using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Migrations
{
    public class MigrationSerializationProvider : IBsonSerializationProvider
    {
        public IBsonSerializer GetSerializer(Type type)
        {
            // TODO: how to filter what can be migrated?
            if (type.FullName.StartsWith("System.") || type.FullName.StartsWith("MongoDB.Bson."))
            {
                return null;
            }

            var classMap = BsonClassMap.LookupClassMap(type);
            if (classMap.ExtraElementsMemberMap != null && 
                classMap.ExtraElementsMemberMap.MemberType == typeof(BsonDocument))
            {
                // the expensive ClassMap was not for nothing created
                // it will be shortly reused by the normal ClassMap Serializer
                return null;
            }
            else
            {
                var versionDetectionStrategy = new AssemblyVersionStrategy(type.Assembly);
                var versionSerializer = new LightweightVersionSerializer();
                return new BsonMigrationSerializer(versionSerializer, versionDetectionStrategy, classMap);
            }
        }
    }
}