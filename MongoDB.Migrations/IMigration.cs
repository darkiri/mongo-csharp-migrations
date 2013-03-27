using System;
using System.Collections.Generic;

namespace MongoDB.Migrations
{
    public interface IMigration
    {
        Version To { get; }
    }

    public interface IMigration<in T> : IMigration
    {
        void Upgrade(T obj, IDictionary<string, object> extraElements);
    }
}