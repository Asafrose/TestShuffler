using MongoDB.Driver;
using System.Linq;

namespace TestShuffler
{
    public interface IDatabase
    {
        IMongoCollection<Document> DocumentsCollection { get; }
    }

    public class Database : Module, IDatabase
    {
        public IMongoCollection<Document> DocumentsCollection { get; }

        public Database()
        {
            var database = new MongoClient("").GetDatabase(nameof(Database));

            if (database.ListCollectionNames().ToList().All(_ => _ != nameof(DocumentsCollection)))
            {
                database.CreateCollection(nameof(DocumentsCollection));
            }

            DocumentsCollection = database.GetCollection<Document>(nameof(DocumentsCollection));
        }
    }
}