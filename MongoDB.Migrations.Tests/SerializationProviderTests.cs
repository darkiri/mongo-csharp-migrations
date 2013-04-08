using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Wrappers;
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
        public void PrimitiveTypesAreNotMigratable()
        {
            Assert.IsNull(GetSerializer(typeof (int)));
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
        public void BsonSpecificTypedShouldNotBeMigratable()
        {
            Assert.IsNull(GetSerializer(typeof (ObjectId)));
            Assert.IsNull(GetSerializer(typeof(BsonDateTime)));
        }

        [Test]
        public void BsonDocumentIsNotMigratable() 
        {
            Assert.IsNull(GetSerializer(typeof (BsonDocument)));
            Assert.IsNull(GetSerializer(typeof (RawBsonDocument)));
            Assert.IsNull(GetSerializer(typeof (QueryDocument)));
            Assert.IsNull(GetSerializer(typeof (CommandDocument)));
        }

        [Test]
        public void BsonSerializableIsNotMigratable() 
        {
            Assert.IsNull(GetSerializer(typeof (BsonDocumentWrapper)));
            Assert.IsNull(GetSerializer(typeof (FieldsWrapper)));
            Assert.IsNull(GetSerializer(typeof (UpdateWrapper)));
        }

        [Test]
        public void TypesWithExtraElementsAsBsonDocumentAreNotMigrated() 
        {
            Assert.IsNull(GetSerializer(typeof (TypeWithExtraElementsBsonDocument)));
        }

        [Test]
        public void OrdinalTypesShouldBeMigratable()
        {
            Assert.That(GetSerializer(typeof(MigratableType)), Is.InstanceOf<BsonMigrationSerializer>());
        }

        [Test]
        public void TypesWithExtraElementsAreMigratable()
        {
            Assert.That(GetSerializer(typeof (TypeWithExtraElements)), Is.InstanceOf<BsonMigrationSerializer>());
        }

        private class MigratableType { }

        private class TypeWithExtraElements
        {
            public Dictionary<string, object> ExtraElements;
        }

        private class TypeWithExtraElementsBsonDocument
        {
            public BsonDocument ExtraElements;
        }
    }
}