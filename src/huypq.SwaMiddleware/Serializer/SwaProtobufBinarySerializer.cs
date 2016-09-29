using System.IO;
using System.IO.Compression;

namespace huypq.SwaMiddleware
{
    public class SwaProtobufBinarySerializer : SwaISerializer
    {
        public T Deserialize<T>(Stream data)
        {
            return ProtoBuf.Serializer.Deserialize<T>(data);
        }

        public T Deserialize<T>(object data)
        {
            using (var ms = new MemoryStream(data as byte[]))
            {
                return ProtoBuf.Serializer.Deserialize<T>(ms);
            }
        }

        public void Serialize(Stream output, object data)
        {
            using (var gzipStream = new GZipStream(output, CompressionLevel.Fastest))
            {
                ProtoBuf.Serializer.Serialize(gzipStream, data);
            }
        }
    }
}
