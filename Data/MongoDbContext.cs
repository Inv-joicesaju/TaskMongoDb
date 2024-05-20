namespace TaskMongoDb.Data
{
    using MongoDB.Driver;
    using TaskMongoDb.Models;

    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<TaskModel> Tasks => _database.GetCollection<TaskModel>("tasks");
        public IMongoCollection<Item> Items => _database.GetCollection<Item>("Items");
    }
}
