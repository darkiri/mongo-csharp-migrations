using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using NUnit.Framework;

namespace MongoDB.Migrations.Tests
{
    [TestFixture]
    public class SerializationProviderTests
    {
        private IBsonSerializer GetSerializer(Type type)
        {
            var provider = new MigrationSerializationProvider();
            return provider.GetSerializer(type);
        }

        [Test]
        public void ArraysAreNotMigratable()
        {
            Assert.IsNull(GetSerializer(typeof (object[])));
        }

        [Test]
        public void EnumsAreNotMigratable()
        {
            
            Assert.IsNull(GetSerializer(typeof (GCCollectionMode)));
        }

        [Test]
        public void TypesWithoutExtraMethodsAreNotMigratable()
        {
            Assert.IsNull(GetSerializer(typeof (NotMigratableType)));
        }

        [Test]
        public void PrimitiveTypesAreNotMigratable()
        {
            Assert.IsNull(GetSerializer(typeof (int)));
        }

        [Test]
        public void TypesWithExtraMethodsAreMigratable()
        {
            Assert.That(GetSerializer(typeof (TypeWithExtraElements)), Is.InstanceOf<BsonMigrationSerializer>());
        }

        private class NotMigratableType {}

        private class TypeWithExtraElements
        {
            public Dictionary<string, object> ExtraElements;
        }
    }
}