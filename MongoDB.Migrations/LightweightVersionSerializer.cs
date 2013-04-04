using System;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Migrations
{
    /// <summary>
    /// Serialize Version as BsonLong: 
    /// 0000 MMMM mmmm BBBB
    /// where MMMM - Major version, mmmm - minor version, BBBB - build number
    /// Revision is ignored
    /// Maximum value per version compnent is 65565
    /// </summary>
    public class LightweightVersionSerializer : BsonBaseSerializer
    {
        public LightweightVersionSerializer() : base(new RepresentationSerializationOptions(BsonType.Int64)) {}

        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof (Version));

            var bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Int64:
                    return DeserializeVersion(bsonReader.ReadInt64());
                default:
                    throw new FileFormatException(string.Format("Cannot deserialize Version from BsonType {0}.", bsonType));
            }
        }

        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            var versionValue = (Version) value;
            var representationSerializationOptions =
                EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            switch (representationSerializationOptions.Representation)
            {
                case BsonType.Int64:
                    bsonWriter.WriteInt64(SerializeVersion(versionValue));
                    break;
                default:
                    throw new BsonSerializationException(string.Format("'{0}' is not a valid Version representation.", representationSerializationOptions.Representation));
            }
        }

        public Version DeserializeVersion(long value)
        {
            return new Version(ExtractVersionComponent(value, 32),
                               ExtractVersionComponent(value, 16),
                               ExtractVersionComponent(value, 0));
        }

        private static int ExtractVersionComponent(long value, int shift)
        {
            return (int) (value >> shift) & VERSION_COMPONENT_MAX;
        }

        public long SerializeVersion(Version version)
        {
            VerifyVersion(version);
            return ((long) version.Major << 32) + ((long) version.Minor << 16) + version.Build;
        }

        private void VerifyVersion(Version version)
        {
            if (version.Major > VERSION_COMPONENT_MAX ||
                version.Minor > VERSION_COMPONENT_MAX ||
                version.Build > VERSION_COMPONENT_MAX)
            {
                throw new BsonSerializationException(
                    String.Format("Version '{0}' cannot be serialized. All version components must be smaller or equal to {1}", version, VERSION_COMPONENT_MAX));
            }
        }

        private const int VERSION_COMPONENT_MAX = 0xFFFF;
    }
}