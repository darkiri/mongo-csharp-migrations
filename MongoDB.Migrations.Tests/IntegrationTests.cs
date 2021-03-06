﻿using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;

namespace MongoDB.Migrations.Tests
{
    [TestFixture]
    public class IntegrationTests : DatabaseTestsBase
    {
        private const string CUSTOMERS_COLLECTION = "customers";
        private MongoCollection<Customer> _customersCollection;

        private class CustomerOld
        {
            public string Name { get; set; }
        }

        [Migration(typeof (MigrationTo_0_5))]
        [Migration(typeof (MigrationTo_1_0))]
        private class Customer
        {
            public ObjectId Id { get; set; }
            public string Title { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        private class MigrationTo_0_5 : IMigration<Customer>
        {
            public Version To
            {
                get { return new Version(0, 5); }
            }

            public void Upgrade(Customer obj, IDictionary<string, object> extraElements)
            {
                var fullName = (string) extraElements["Name"];
                obj.LastName = fullName.Split().Last();
                obj.FirstName = fullName.Substring(0, fullName.Length - obj.LastName.Length).Trim();
                extraElements.Remove("Name");
            }
        }

        private class MigrationTo_1_0 : IMigration<Customer>
        {
            public Version To
            {
                get { return new Version(1, 0); }
            }

            public void Upgrade(Customer obj, IDictionary<string, object> extraElements)
            {
                obj.Title = obj.FirstName.Split().Count() > 1 ? obj.FirstName.Split().First() : null;
                obj.FirstName = obj.FirstName.Substring((obj.Title ?? "").Length).Trim();
            }
        }

        [SetUp]
        public void SetUp()
        {
            BsonSerializer.RegisterSerializationProvider(new MigrationSerializationProvider());

            SetUpDatabase();
            _customersCollection = GetDatabaseCollection<Customer>(CUSTOMERS_COLLECTION);
        }

        [Test]
        public void TestMigrations()
        {
            _customersCollection.Insert(new CustomerOld {Name = "Miss Chanandler Bong"});

            var customer = _customersCollection.AsQueryable().First();
            _customersCollection.Save(customer);

            customer = _customersCollection.AsQueryable().First();
            Assert.That(customer.Title, Is.EqualTo("Miss"));
            Assert.That(customer.FirstName, Is.EqualTo("Chanandler"));
            Assert.That(customer.LastName, Is.EqualTo("Bong"));
        }
    }
}