using System;
using System.Collections.Generic;

namespace MongoDB.Migrations
{
    public interface IMigration<in T>
    {
        Version To { get; }
        void Upgrade(T obj, IDictionary<string, object> extraElements);
    }
}