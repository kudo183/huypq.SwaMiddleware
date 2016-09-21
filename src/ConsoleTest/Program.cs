using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Main " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            Task.Run(async () =>
            {
                //string host = "http://localhost:5000/test/get?id=5";
                //string host = "http://localhost:5000/test/getimage";
                string host = "http://localhost:5000/test/getbytes";

                var getProtoBufTask = Get(host, "protobuf");
                var getJsonTask = Get(host, "json");
                
                Console.WriteLine("do something");

                Console.WriteLine(await getJsonTask);
                Console.WriteLine(await getProtoBufTask);
                Console.Read();
            }).Wait();
        }

        public static async Task<long> Get(string uri, string responseType)
        {
            Console.WriteLine("get " + responseType);
            var client = System.Net.WebRequest.CreateHttp(uri);
            client.Headers["response"] = responseType;
            var response = await client.GetResponseAsync();
            var responseStream = response.GetResponseStream();

            var ms = new MemoryStream();
            responseStream.CopyTo(ms, 4096);
            ms.Position = 0;
            if (responseType == "protobuf")
            {
                var r = ProtoBuf.Serializer.Deserialize<List<TestData>>(ms);
            }
            else
            {
                var serialize = Newtonsoft.Json.JsonSerializer.Create();
                var sr = new StreamReader(ms);
                var rd = new Newtonsoft.Json.JsonTextReader(sr);
                var t = serialize.Deserialize<List<TestData>>(rd);
            }
            return ms.Length;
        }

        [ProtoBuf.ProtoContract]
        public class TestData
        {
            [ProtoBuf.ProtoMember(1)]
            public string Name { get; set; }
            [ProtoBuf.ProtoMember(2)]
            public string ContentOfThisObject { get; set; }
            [ProtoBuf.ProtoMember(3)]
            public int Number { get; set; }
            [ProtoBuf.ProtoMember(4)]
            public List<int> ListInt { get; set; }
            [ProtoBuf.ProtoMember(5)]
            public System.DateTime Date { get; set; }
        }
    }
}
