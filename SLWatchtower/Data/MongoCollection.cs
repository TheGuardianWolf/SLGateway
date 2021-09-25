using MongoDB.Driver;

namespace SLWatchtower.Data
{
    public interface IMongoDataSource
    {
        IMongoDatabase Database { get; }
    }

    public class MongoDataSource : IMongoDataSource
    {
        public IMongoDatabase Database { get; }

        public MongoDataSource(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(databaseName);
        }
    }
}
