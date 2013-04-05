using MongoDB.Driver;

namespace MongoDB.Migrations.Tests
{
    public class DatabaseTestsBase {
        private static MongoDatabase _db;
        private const string TEST_DATABASE_NAME = "mongodb_migrations_tests";

        protected MongoCollection<TCollection> GetDatabaseCollection<TCollection>(string collectionName)
        {
            return _db.GetCollection<TCollection>(collectionName);
        }

        public static MongoDatabase SetUpDatabase()
        {
            var client = new MongoClient("mongodb://localhost/?w=1");
            var server = client.GetServer();
            _db = server.GetDatabase(TEST_DATABASE_NAME);
            _db.Drop();
            return _db;
        }
    }
}