using System;

namespace MongoDB.Migrations
{
    /// <summary>
    /// What ist the actual version of the application (of all data objects of the application).
    /// All touched database documents will be migrated to this version.
    /// Other possibilities:
    ///     - current version per object type
    ///     - current version per object instance
    /// </summary>
    public interface IVersionDetectionStrategy
    {
        Version GetCurrentVersion();
    }
}