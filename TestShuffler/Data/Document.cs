using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TestShuffler
{
    [BsonKnownTypes(typeof(Course),typeof(ServiceProfile))]
    public abstract class Document
    {
        [BsonId]
        public abstract string Id { get; }
    }
}