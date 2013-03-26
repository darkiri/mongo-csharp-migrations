using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using NUnit.Framework;

namespace MongoDB.Migrations.Tests
{
    [TestFixture]
    public class LightwightVersionSerializerTests
    {
        [Test]
        public void MajorShouldBeSerialized()
        {
            AssertVersionSerialized(new Version("1.0.0.0"), 0x100000000);
            AssertVersionSerialized(new Version("65535.0.0.0"), 0xFFFF00000000);
        }

        [Test]
        public void MinorShouldBeSerialized()
        {
            AssertVersionSerialized(new Version("0.1.0.0"), 0x10000);
            AssertVersionSerialized(new Version("0.65535.0.0"), 0xFFFF0000);
        }

        [Test]
        public void BuildShouldBeSerialized()
        {
            AssertVersionSerialized(new Version("0.0.1.0"), 0x1);
            AssertVersionSerialized(new Version("0.0.65535.0"), 0xFFFF);
        }

        [Test]
        public void RevisionShouldNotBeSerialized()
        {
            AssertVersionSerialized(new Version("0.0.0.1"), 0);
        }

        [Test]
        public void AllComponentsShouldBeSerialized()
        {
            AssertVersionSerialized(new Version("65534.65533.65532.0"), 0xFFFEFFFDFFFC);
            AssertVersionSerialized(new Version("65535.65535.65535.0"), 0xFFFFFFFFFFFF);
        }

        [Test, ExpectedException(typeof (BsonSerializationException))]
        public void ComponentsOver65535CannotBeSerialized()
        {
            using (var stringWriter = new StringWriter())
            {
                using (var bsonWriter = BsonWriter.Create(stringWriter))
                {
                    new LightweightVersionSerializer().Serialize(bsonWriter,
                                                                 typeof (Version),
                                                                 new Version(65536, 0),
                                                                 null);
                }
            }
        }

        private static void AssertVersionSerialized(Version version, long serialized)
        {
            var serializer = new LightweightVersionSerializer();
            using (var stringWriter = new StringWriter())
            {
                using (var bsonWriter = BsonWriter.Create(stringWriter))
                {
                    serializer.Serialize(bsonWriter, typeof (Version), version, null);
                }
                Assert.That(stringWriter.ToString(), Is.StringMatching(String.Format("NumberLong\\(\\\"?{0}\\\"?\\)", serialized)));
            }
        }
    }
}