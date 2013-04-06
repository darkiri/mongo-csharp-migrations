using System;
using System.Reflection;

namespace MongoDB.Migrations
{
    public class AssemblyVersionStrategy : IVersionDetectionStrategy
    {
        private readonly Version _assemblyVersion;
        public AssemblyVersionStrategy(Assembly assembly)
        {
            _assemblyVersion = assembly.GetName().Version;
        }

        public Version GetCurrentVersion()
        {
            return _assemblyVersion;
        }
    }
}