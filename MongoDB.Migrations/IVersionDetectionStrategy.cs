using System;

namespace MongoDB.Migrations
{
    public interface IVersionDetectionStrategy
    {
        Version GetCurrentVersion();
    }
}