using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Migrations.Tests
{
    public class MigrationSerializerTestBase {
        protected static string Serialize<T>(T obj, string version)
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

        protected static T Deserialize<T>(string json, string version)
        {
            var serializer = CreateSerializer<T>(version);
            using (var bsonReader = BsonReader.Create(json))
            {
                return (T) serializer.Deserialize(bsonReader, typeof (T), null);
            }
        }

        private static BsonMigrationSerializer CreateSerializer<T>(string version)
        {
            var versionDetection = new SpecificVersionStrategy(version);
            var classMap = BsonClassMap.LookupClassMap(typeof (T));
            return new BsonMigrationSerializer(new VersionSerializer(), versionDetection, classMap);
        }
    }
}