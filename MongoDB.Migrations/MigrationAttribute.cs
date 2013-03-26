using System;

namespace MongoDB.Migrations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MigrationAttribute : Attribute
    {
        private readonly Type _migrationType;

        public MigrationAttribute(Type migrationType)
        {
            _migrationType = migrationType;
        }

        public Type MigrationType
        {
            get { return _migrationType; }
        }
    }
}