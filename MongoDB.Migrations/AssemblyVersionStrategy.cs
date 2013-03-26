using System;
using System.Reflection;

namespace MongoDB.Migrations
{
    public class AssemblyVersionStrategy : IVersionDetectionStrategy
    {
        private readonly Assembly _assembly;
        public AssemblyVersionStrategy(Assembly assembly)
        {
            _assembly = assembly;
        }

        public Version GetCurrentVersion()
        {
            return _assembly.GetName().Version;
        }
    }
}