using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NUnit.Framework;

namespace MongoDB.Migrations.Tests
{
    [TestFixture, Explicit]
    public class PerformanceTests : DatabaseTestsBase
    {
        private const string PERSONS_COLLECTION = "people";
        private const int COLLECTION_SIZE = 100000;

        private class PersonWithoutMigration1
        {
            public ObjectId Id;
            public string FullName;
            public string Title;
        }

        private class PersonWithoutMigration2
        {
            public ObjectId Id;
            public string FullName;
            public string Title;
        }

        [Migration(typeof (EmptyMigration))]
        private class PersonWithMigration
        {
            public ObjectId Id;
            public string FullName;
            public string Title;
        }

        private class EmptyMigration : IMigration<PersonWithMigration>
        {
            public Version To
            {
                get { return new Version(0, 1); }
            }

            public void Upgrade(PersonWithMigration obj, IDictionary<string, object> extraElements) {}
        }

        [SetUp]
        public void SetUp()
        {
            SetUpDatabase();

            var persons = GetDatabaseCollection<PersonWithoutMigration1>(PERSONS_COLLECTION);
            Console.Out.WriteLine("Creating test collection...");
            var generator = new NamesGenerator();
            for (var i = 1; i <= COLLECTION_SIZE; i++)
            {
                var person = new PersonWithoutMigration1
                {
                    FullName = generator.GetName(6) + " " + generator.GetName(10),
                    Title = generator.GetName(5),
                };
                persons.Insert(person);
                if (i % 10000 == 0)
                {
                    Console.Out.WriteLine("{0} documents created", i);
                }
            }
        }

        [Test]
        public void DeserializationTest()
        {
            var persons1 = GetDatabaseCollection<PersonWithoutMigration1>(PERSONS_COLLECTION);
            var persons2 = GetDatabaseCollection<PersonWithoutMigration2>(PERSONS_COLLECTION);
            var persons3 = GetDatabaseCollection<PersonWithMigration>(PERSONS_COLLECTION);

            RepeatMeasurement(3, "BsonClassMapSerializer", () => persons1.FindAll().ToArray());
            BsonSerializer.RegisterSerializationProvider(new MigrationSerializationProvider());
            RepeatMeasurement(3, "BsonMigrationSerializer, type without migrations", () => persons2.FindAll().ToArray());
            RepeatMeasurement(3, "BsonMigrationSerializer, type with empty migrations", () => persons3.FindAll().ToArray());
        }

        private void RepeatMeasurement(int times, string message, Action test)
        {
            Console.Out.WriteLine();
            Console.Out.WriteLine(message);

            for (var i = 0; i < times; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                test();
                stopwatch.Stop();
                Console.Out.WriteLine(stopwatch.Elapsed);
            }
        }

        private class NamesGenerator
        {
            private readonly Random _random = new Random();
            private readonly char[] _vowels = new[] {'A', 'E', 'I', 'O', 'U', 'Y', 'a', 'e', 'i', 'o', 'u', 'y'};

            public string GetName(int length)
            {
                var name = "" + GetConsonant(true);
                for (int i = 1; i < length; i++)
                {
                    name += i % 2 == 0
                                ? GetConsonant(false)
                                : GetVowel();
                }
                return name;
            }

            private char GetVowel()
            {
                return _vowels[_random.Next(6, 12)];
            }

            private char GetConsonant(bool upperCase)
            {
                var ch = upperCase
                             ? (char) _random.Next(66, 91)
                             : (char) _random.Next(98, 123);
                return IsVowel(ch) ? GetConsonant(upperCase) : ch;
            }

            private bool IsVowel(char c)
            {
                return _vowels.Any(v => v == c);
            }
        }
    }
}