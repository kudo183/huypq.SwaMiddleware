using System.IO;

namespace huypq.SwaMiddleware
{
    public class SwaProtobufBinarySerializer : SwaISerializer
    {
        public T Deserialize<T>(object data)
        {
            using (var ms = new MemoryStream(data as byte[]))
            {
                return ProtoBuf.Serializer.Deserialize<T>(ms);
            }
        }

        public void Serialize(Stream output, object data)
        {
            ProtoBuf.Serializer.Serialize(output, data);
        }
    }
}
