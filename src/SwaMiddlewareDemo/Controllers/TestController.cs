using huypq.SwaMiddleware;
using System.Collections.Generic;

namespace SwaMiddlewareDemo.Controllers
{
    public class TestController : SwaController
    {
        public override object ActionInvoker(string actionName, Dictionary<string, object> parameter)
        {
            object result = null;

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
                default:
                    break;
            }

            return result;
        }

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2", TokenModel.User, TokenModel.CreateTime.Ticks.ToString() };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }
    }
}
