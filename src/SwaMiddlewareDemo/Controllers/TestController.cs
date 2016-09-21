using huypq.SwaMiddleware;
using System.Collections.Generic;

namespace SwaMiddlewareDemo.Controllers
{
    public class TestController : SwaController
    {
        public override SwaActionResult ActionInvoker(string actionName, Dictionary<string, object> parameter)
        {
            SwaActionResult result = null;

            switch (actionName)
            {
                case "get":
                    switch (parameter.Count)
                    {
                        case 0:
                            result = Get();
                            break;
                        case 1:
                            int id = int.Parse(parameter["id"].ToString());
                            result = Get(id);
                            break;
                    }
                    break;
                case "getimage":
                    result = GetImage();
                    break;
                case "getfile":
                    result = GetFile();
                    break;
                case "getbytes":
                    result = GetBytes();
                    break;
                default:
                    break;
            }

            return result;
        }

        public SwaActionResult Get()
        {
            return CreateJsonResult(
                new string[] { "value1", "value2", TokenModel.User, TokenModel.CreateTime.Ticks.ToString() });
        }

        // GET api/values/5
        public SwaActionResult Get(int id)
        {
            return CreateJsonResult("value");
        }

        public SwaActionResult GetImage()
        {
            var fileName = "image.jpg";
            var path = System.IO.Path.GetFullPath(fileName);
            var stream = new System.IO.FileStream(path, System.IO.FileMode.Open);
            return CreateStreamResult(stream, MimeMapping.GetMimeType(fileName));
        }

        public SwaActionResult GetFile()
        {
            var fileName = "test.txt";
            System.IO.MemoryStream stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("this is a text file"));

            return CreateFileResult(stream, fileName);
        }

        public SwaActionResult GetBytes()
        {
            var result = new List<TestData>();
            result.Add(new TestData()
            {
                Name = "n1",
                ContentOfThisObject = "content1",
                Number = 123,
                ListInt = new List<int> { 1, 2, 3, 1, 2, 3 },
                Date = System.DateTime.Now
            });
            return CreateObjectResult(result);
        }
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
