using MongoDB.Driver;
using TaskMongoDb.Models;

namespace TaskMongoDb.Data
{
    public interface IMongoDbContext
    {
        IMongoCollection<Item> Items { get; }
        IMongoCollection<TaskModel> Tasks { get; }
    }

}
