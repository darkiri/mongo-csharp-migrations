using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MongoDB.Migrations.Tests
{
    [TestFixture]
    public class MigrationSerializerTests : MigrationSerializerTestBase
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
        public void ClassWithoutMigrationsShouldNotSerializeVersion()
        {
            var json = Serialize(new NotYetMigratableClass(), "1.1.0.1");
            Assert.That(json, Is.EqualTo("{ \"Bla\" : 0 }"));
        }

        public class NotYetMigratableClass
        {
            public int Bla;
        }

        [Test]
        public void ExtraElementsShouldNotContainVersionInformation()
        {
            var obj = Deserialize<WithSingleUpgrade>("{ \"Bla\" : 1, \"_v\" : \"1.1.0.0\" }", "1.1.0");
            Assert.False(obj.ExtraElements.ContainsKey("_v"));
        }

        [Test]
        public void ForDocumentsWithoutVersionShouldBeAppliedAllUpgrades()
        {
            var obj = Deserialize<WithSingleUpgrade>("{ \"Bla\" : 1 }", "1.1.1");
            Assert.That(obj.Bla, Is.EqualTo(-1));
        }

        [Test]
        public void UpgradesForNewVersionsShouldBeApplied()
        {
            var obj = Deserialize<WithSingleUpgrade>("{ \"Bla\" : 1, \"_v\" : \"1.1.0.0\" }", "1.1.1");
            Assert.That(obj.Bla, Is.EqualTo(-1));
        }

        [Test]
        public void UpgradesForOldVersionsShouldNotBeApplied()
        {
            var obj = Deserialize<WithSingleUpgrade>("{ \"Bla\" : 1, \"_v\" : \"1.1.0.0\" }", "1.0.1");
            Assert.That(obj.Bla, Is.EqualTo(1));
        }

        [Migration(typeof (MigrationTo_1_1_1_SettingBlaToMinus1))]
        public class WithSingleUpgrade
        {
            public int Bla;
            public Dictionary<string, object> ExtraElements;
        }

        [Test]
        public void UpgradesInClassWithoutExtraElementsShouldBeApplied()
        {
            var obj = Deserialize<WithSingleUpgradeWithoutExtraElements>("{ \"Bla\" : 1, \"_v\" : \"1.1.0.0\" }", "1.1.1");
            Assert.That(obj.Bla, Is.EqualTo(-1));
        }

        [Migration(typeof (MigrationTo_1_1_1_SettingBlaToMinus1))]
        public class WithSingleUpgradeWithoutExtraElements
        {
            public int Bla;
        }

        [Test, ExpectedException(typeof (MigrationException))]
        public void SeveralMigrationsForSameVersionShouldAbortThePipeline()
        {
            Deserialize<SampleClass1>("{ \"Bla\" : 1, \"_v\" : \"1.1.0.0\" }", "2.2.1");
        }

        [Migration(typeof (MigrationTo_2_1_1))]
        [Migration(typeof (AnotherMigrationTo_2_1_1))]
        public class SampleClass1 
        { 
            public Dictionary<string, object> ExtraElements;
        }

        public class MigrationTo_2_1_1 : IMigration<SampleClass1>
        {
            public Version To
            {
                get { return new Version(2, 1, 1); }
            }

            public void Upgrade(SampleClass1 obj, IDictionary<string, object> extraElements) {}
        }

        public class AnotherMigrationTo_2_1_1 : IMigration<SampleClass1>
        {
            public Version To
            {
                get { return new Version(2, 1, 1); }
            }

            public void Upgrade(SampleClass1 obj, IDictionary<string, object> extraElements) {}
        }

        [Test]
        public void CurrentApplicationVersionShouldRestrictUpgrades()
        {
            var obj = Deserialize<SampleClass>("{ \"Bla\" : 1, \"_v\" : \"1.1.0.0\" }", "1.1.1");
            Assert.That(obj.Bla, Is.EqualTo(-1));
        }

        [Test]
        public void ExceptionsInUpgradesShouldBeHandled()
        {
            try
            {
                Deserialize<SampleClass>("{ \"Bla\" : 1, \"_v\" : \"1.1.0.0\" }", "1.3.1");
                Assert.Fail("Expected " + typeof (MigrationException));
            }
            catch (MigrationException e)
            {
                Assert.That(e.AbortedVersion, Is.EqualTo(new Version(1, 3, 1)));
                Assert.That(e.MigratedType, Is.EqualTo(typeof (SampleClass)));
            }
            catch (Exception another)
            {
                Assert.Fail("Expected " + typeof (MigrationException) + " but thrown " + another.GetType());
            }
        }


        [Migration(typeof (MigrationTo_1_1_1_SettingBlaToMinus1))]
        [Migration(typeof (MigrationTo_1_1_2_SettingBlaToMinus2))]
        [Migration(typeof (MigrationTo_1_3_1_WithException))]
        public class SampleClass
        {
            public int Bla;
            public Dictionary<string, object> ExtraElements;
        }

        public class MigrationTo_1_3_1_WithException : IMigration<SampleClass>
        {
            public Version To
            {
                get { return new Version(1, 3, 1); }
            }

            public void Upgrade(SampleClass obj, IDictionary<string, object> extraElements)
            {
                throw new Exception();
            }
        }

        public class MigrationTo_1_1_2_SettingBlaToMinus2 : IMigration<SampleClass>
        {
            public Version To
            {
                get { return new Version(1, 1, 2); }
            }

            public void Upgrade(SampleClass obj, IDictionary<string, object> extraElements)
            {
                obj.Bla = -2;
            }
        }

        public class MigrationTo_1_1_1_SettingBlaToMinus1 : IMigration<SampleClass>, IMigration<WithSingleUpgrade>, IMigration<WithSingleUpgradeWithoutExtraElements>
        {
            public Version To
            {
                get { return new Version(1, 1, 1); }
            }

            public void Upgrade(WithSingleUpgrade obj, IDictionary<string, object> extraElements)
            {
                obj.Bla = -1;
            }

            public void Upgrade(SampleClass obj, IDictionary<string, object> extraElements)
            {
                obj.Bla = -1;
            }

            public void Upgrade(WithSingleUpgradeWithoutExtraElements obj, IDictionary<string, object> extraElements)
            {
                obj.Bla = -1;
            }
        }
    }
}