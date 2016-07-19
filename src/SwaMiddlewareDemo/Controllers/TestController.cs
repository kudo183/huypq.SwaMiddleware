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
    }
}
