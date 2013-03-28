using System;

namespace MongoDB.Migrations
{
    public class MigrationException : Exception
    {
        public MigrationException(Type type, Version abortedVersion) : this(type, abortedVersion, null) {}

        public MigrationException(Type type, Version abortedVersion, Exception innerException) :
            base(FormatMessage(type, abortedVersion), innerException)
        {
            AbortedVersion = abortedVersion;
            MigratedType = type;
        }

        public Version AbortedVersion { get; private set; }
        public Type MigratedType { get; private set; }

        private static string FormatMessage(Type type, Version abortedVersion)
        {
            return String.Format("Migration of an object of type '{0}' failed during upgrade to Version '{1}'",
                                 type,
                                 abortedVersion);
        }
    }
}