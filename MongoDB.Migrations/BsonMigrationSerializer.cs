using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Migrations
{
    public class BsonMigrationSerializer : BsonClassMapSerializer
    {
        private readonly IVersionDetectionStrategy _versionDetectionStrategy;
        private readonly IEnumerable<IMigration> _migrations;
        private const string VERSION_ELEMENT_NAME = "_v";
        private readonly IBsonSerializer _versionSerializer;

        public BsonMigrationSerializer(IBsonSerializer versionSerializer, IVersionDetectionStrategy versionDetectionStrategy, BsonClassMap classMap) : base(classMap)
        {
            _versionSerializer = versionSerializer;
            _versionDetectionStrategy = versionDetectionStrategy;
            _migrations = ExtractMigrations(classMap.ClassType);
        }

        private IEnumerable<IMigration> ExtractMigrations(Type classType)
        {
            var migrationTypes = classType
                .GetCustomAttributes(typeof (MigrationAttribute), false)
                .Cast<MigrationAttribute>()
                .Select(a => a.MigrationType)
                .ToArray();

            var migrationInterface = typeof (IMigration<>);
            migrationInterface = migrationInterface.MakeGenericType(new[] { classType });

            if (migrationTypes.Any(t => t.GetInterfaces().All(i => i != migrationInterface)))
            {
                throw new ArgumentException("One of migration types is not a subclass of " + migrationInterface.Name);
            }
            return migrationTypes
                .Select(Activator.CreateInstance)
                .Cast<IMigration>()
                .OrderBy(m => m.To)
                .ToArray();
        }

        protected override void OnSerialized(BsonWriter bsonWriter, object value, IBsonSerializationOptions options)
        {
            if (_migrations.Any())
            {
                bsonWriter.WriteName(VERSION_ELEMENT_NAME);
                var currentVersion = _versionDetectionStrategy.GetCurrentVersion();
                _versionSerializer.Serialize(bsonWriter, typeof(Version), currentVersion, null);
            }
        }

        protected override void OnMemberNotFound(BsonReader bsonReader, object obj, string elementName, Dictionary<string, object> notFoundElements)
        {
            if (elementName == VERSION_ELEMENT_NAME)
            {
                notFoundElements[VERSION_ELEMENT_NAME] = _versionSerializer.Deserialize(bsonReader, typeof(Version), null);
            }
            else
            {
                var bsonValue = (BsonValue) BsonValueSerializer.Instance.Deserialize(bsonReader, typeof (BsonValue), null);
                notFoundElements[elementName] = BsonTypeMapper.MapToDotNetValue(bsonValue);
            }
        }

        protected override void DeserializeExtraElement(BsonReader bsonReader, object obj, string elementName, BsonMemberMap extraElementsMemberMap)
        {
            if (elementName == VERSION_ELEMENT_NAME)
            {
                var extraElements = EnsureExtraElements(obj, extraElementsMemberMap);
                extraElements[VERSION_ELEMENT_NAME] = _versionSerializer.Deserialize(bsonReader, typeof (Version), null);
            }
            else
            {
                base.DeserializeExtraElement(bsonReader, obj, elementName, extraElementsMemberMap);
            }
        }

        private static IDictionary<string, object> EnsureExtraElements(object obj, BsonMemberMap extraElementsMemberMap)
        {
            // TODO: make this method protected non-static and pull up?
            var extraElements = (IDictionary<string, object>) extraElementsMemberMap.Getter(obj);
            if (extraElements == null)
            {
                if (extraElementsMemberMap.MemberType == typeof (IDictionary<string, object>))
                {
                    extraElements = new Dictionary<string, object>();
                }
                else
                {
                    extraElements = (IDictionary<string, object>) Activator.CreateInstance(extraElementsMemberMap.MemberType);
                }
                extraElementsMemberMap.Setter(obj, extraElements);
            }
            return extraElements;
        }


        protected override void OnDeserialized(object obj, IDictionary<string, object> notFoundElements)
        {
            var extraElements = _classMap.ExtraElementsMemberMap == null
                ? notFoundElements
                : (IDictionary<string, object>) _classMap.ExtraElementsMemberMap.Getter(obj);

            object objectVersion;
            if (null != extraElements && extraElements.ContainsKey(VERSION_ELEMENT_NAME))
            {
                objectVersion = extraElements[VERSION_ELEMENT_NAME];
                extraElements.Remove(VERSION_ELEMENT_NAME);
            }
            else
            {
                objectVersion = new Version(0, 0);
            }

            RunUpgrades((Version) objectVersion, obj, extraElements);
        }

        private void RunUpgrades(Version objectVersion, object obj, IDictionary<string, object> extraElements)
        {
            var filteredMigrations = FilterMigrations(objectVersion);
            EnsureNoDuplicates(obj, filteredMigrations);

            foreach (var migration in filteredMigrations)
            {
                try
                {
                    InvokeUpgrade(migration, obj, extraElements);
                }
                catch (Exception e)
                {
                    throw new MigrationException(obj.GetType(), migration.To, e);
                }
            }
        }

        private IMigration[] FilterMigrations(Version fromVersion)
        {
            var currentVersion = _versionDetectionStrategy.GetCurrentVersion();
            return _migrations
                .Where(m => m.To > fromVersion && m.To <= currentVersion)
                .ToArray();
        }

        private static void EnsureNoDuplicates(object obj, IEnumerable<IMigration> filteredMigrations)
        {
            var duplicate = filteredMigrations
                .GroupBy(m => m.To)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null) 
                throw new MigrationException(obj.GetType(), duplicate.First().To);
        }

        private static void InvokeUpgrade(IMigration migration, object obj, IDictionary<string, object> extraElements)
        {
            var upgrade = migration
                .GetType()
                .GetMethod("Upgrade", new[] {obj.GetType(), typeof (IDictionary<string, object>)});
            upgrade.Invoke(migration, new[] {obj, extraElements});
        }
    }
}