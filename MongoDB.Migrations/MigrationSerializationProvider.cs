using System;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace MongoDB.Migrations
{
    public class MigrationSerializationProvider : IBsonSerializationProvider
    {
        public IBsonSerializer GetSerializer(Type type)
        {
            if (type.IsPrimitive ||
                typeof (Array).IsAssignableFrom(type) ||
                typeof (Enum).IsAssignableFrom(type))
            {
                return null;
            }
            else
            {
                var classMap = BsonClassMap.LookupClassMap(type);
                if (classMap.ExtraElementsMemberMap == null)
                {
                    // the expensive ClassMap was not for nothing created
                    // it will be shortly reused by the normal ClassMap Serializer
                    return null;
                }
                else
                {
                    var versionDetectionStrategy = new AssemblyVersionStrategy(type.Assembly);
                    var versionSerializer = new LightweightVersionSerializer();
                    return new BsonMigrationSerializer(versionSerializer, versionDetectionStrategy, classMap);
                }
            }
        }
    }
}