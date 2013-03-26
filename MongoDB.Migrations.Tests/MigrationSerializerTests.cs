using System;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using NUnit.Framework;

namespace MongoDB.Migrations.Tests
{
    [TestFixture]
    public class MigrationSerializerTests
    {
        [Test]
        public void ShouldSerializeOrdinalFields()
        {
            var json = Serialize(new SampleClass {Bla = 5}, "1.1.0.1");
            Assert.That(json, Is.StringMatching("\"Bla\" : 5"));
        }

        [Test]
        public void ShouldSerializeCurrentVersion()
        {
            var json = Serialize(new SampleClass(), "1.1.0.1");
            Assert.That(json, Is.EqualTo("{ \"Bla\" : 0, \"_v\" : \"1.1.0.1\" }"));
        }

        [Test]
        public void NotMigratableClassShouldNotSerializeVersion()
        {
            var json = Serialize(new NotMigratableClass(), "1.1.0.1");
            Assert.That(json, Is.EqualTo("{ \"Bla\" : 0 }"));
        }

        [Test]
        public void UpgradesShouldBeApplied()
        {
            var obj = Deserialize<WithSingleUpgrade>("{ \"Bla\" : 1, \"_v\" : \"1.1.0.0\" }", "1.1.1.0");
            Assert.That(obj.Bla, Is.EqualTo(-1));
        }

        private static string Serialize<T>(T obj, string version)
        {
            var serializer = CreateSerializer<T>(version);
            using (var stringWriter = new StringWriter())
            {
                using (var bsonWriter = BsonWriter.Create(stringWriter))
                {
                    serializer.Serialize(bsonWriter,
                                         obj.GetType(),
                                         obj,
                                         DocumentSerializationOptions.SerializeIdFirstInstance);
                }
                return stringWriter.ToString();
            }
        }

        private static BsonMigrationSerializer CreateSerializer<T>(string version)
        {
            var versionDetection = new SpecificVersionStrategy(version);
            var classMap = BsonClassMap.LookupClassMap(typeof (T));
            var serializer = new BsonMigrationSerializer(new VersionSerializer(), versionDetection, classMap);
            return serializer;
        }

        private static T Deserialize<T>(string json, string version)
        {
            var serializer = CreateSerializer<T>(version);
            using (var bsonReader = BsonReader.Create(json))
            {
                return (T) serializer.Deserialize(bsonReader, typeof (T), null);
            }
        }

        [Migration(typeof (UpgradeTo_1_1_1))]
        [Migration(typeof (UpgradeTo_1_1_2))]
        [Migration(typeof (UpgradeTo_1_3_1))]
        public class SampleClass
        {
            public int Bla;
            public Dictionary<string, object> ExtraElements;
        }

        [Migration(typeof (UpgradeTo_1_1_1))]
        public class WithSingleUpgrade
        {
            public int Bla;
            public Dictionary<string, object> ExtraElements;
        }

        public class NotMigratableClass
        {
            public int Bla;
        }

        public class UpgradeTo_1_3_1 : IMigration<SampleClass>
        {
            public Version To
            {
                get { return new Version(1, 3, 1); }
            }

            public void Upgrade(SampleClass obj, Dictionary<string, object> extraElements) {}
        }

        public class UpgradeTo_1_1_2 : IMigration<SampleClass>
        {
            public Version To
            {
                get { return new Version(1, 1, 2); }
            }

            public void Upgrade(SampleClass obj, Dictionary<string, object> extraElements) {}
        }

        public class UpgradeTo_1_1_1 : IMigration<SampleClass>, IMigration<WithSingleUpgrade>
        {

            public Version To
            {
                get { return new Version(1, 1, 1); }
            }

            public void Upgrade(WithSingleUpgrade obj, Dictionary<string, object> extraElements)
            {
                obj.Bla = -1;
            }

            public void Upgrade(SampleClass obj, Dictionary<string, object> extraElements) {}
        }
    }
}