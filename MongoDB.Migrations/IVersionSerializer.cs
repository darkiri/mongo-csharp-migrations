using System;
using MongoDB.Bson.Serialization;

namespace MongoDB.Migrations
{
    public interface IVersionSerializer : IBsonSerializer
    {
        Version DeserializeVersion(long value);
        long SerializeVersion(Version version);
    }
}