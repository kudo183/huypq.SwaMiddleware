﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.IO.Compression;

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

        public T Deserialize<T>(Stream data)
        {
            using (var s = new StreamReader(data))
            using (var sr = new StringReader(s.ReadToEnd()))
            using (var jr = new JsonTextReader(sr))
            {
                return _jsonSerializer.Deserialize<T>(jr);
            }
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
            using (var gzipStream = new GZipStream(output, CompressionLevel.Fastest))
            using (var writer = new StreamWriter(gzipStream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                _jsonSerializer.Serialize(jsonWriter, data);
                writer.Flush();
            }
        }
    }
}
