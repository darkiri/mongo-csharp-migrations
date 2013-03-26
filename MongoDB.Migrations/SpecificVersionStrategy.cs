using System;

namespace MongoDB.Migrations
{
    public class SpecificVersionStrategy : IVersionDetectionStrategy
    {
        private readonly Version _version;
        public SpecificVersionStrategy(Version version)
        {
            _version = version;
        }

        public SpecificVersionStrategy(string version)
        {
            _version = Version.Parse(version);
        }

        public Version GetCurrentVersion()
        {
            return _version;
        }
    }
}