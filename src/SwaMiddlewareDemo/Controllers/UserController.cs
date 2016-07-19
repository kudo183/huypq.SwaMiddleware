using huypq.SwaMiddleware;
using System.Collections.Generic;

namespace SwaMiddlewareDemo.Controllers
{
    public class UserController : SwaController
    {
        public override SwaActionResult ActionInvoker(string actionName, Dictionary<string, object> parameter)
        {
            SwaActionResult result = null;

            switch (actionName)
            {
                case "token":
                    result = Token(parameter["user"].ToString(), parameter["password"].ToString());
                    break;
                case "register":
                    result = Register(parameter["user"].ToString(), parameter["password"].ToString());
                    break;
                default:
                    break;
            }

            return result;
        }

        public SwaActionResult Token(string user, string pass)
        {
            return CreateJsonResult(new SwaTokenModel() { User = user });
        }

        public SwaActionResult Register(string user, string pass)
        {
            return CreateJsonResult(string.Format("{0} - {1}", user, pass));
        }
    }
}
