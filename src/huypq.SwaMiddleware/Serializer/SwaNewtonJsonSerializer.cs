using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace huypq.SwaMiddleware
{
    public class SwaNewtonJsonSerializer : SwaISerializer
    {
        private JsonSerializer _jsonSerializer;

        public SwaNewtonJsonSerializer()
        {
            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            _jsonSerializer = JsonSerializer.Create(settings);
        }

        public T Deserialize<T>(object data)
        {
            using (var sr = new StringReader(data as string))
            using (var jr = new JsonTextReader(sr))
            {
                return _jsonSerializer.Deserialize<T>(jr);
            }
        }

        public void Serialize(Stream output, object data)
        {
            using (var writer = new StreamWriter(output))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                _jsonSerializer.Serialize(jsonWriter, data);
                writer.Flush();
            }
        }
    }
}
