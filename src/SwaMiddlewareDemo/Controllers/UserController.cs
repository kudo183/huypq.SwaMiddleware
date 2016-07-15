using huypq.SwaMiddleware;
using System.Collections.Generic;

namespace SwaMiddlewareDemo.Controllers
{
    public class UserController : SwaController
    {
        public override object ActionInvoker(string actionName, Dictionary<string, object> parameter)
        {
            object result = null;

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

        public SwaTokenModel Token(string user, string pass)
        {
            return new SwaTokenModel() { User = user };
        }

        public string Register(string user, string pass)
        {
            return string.Format("{0} - {1}", user, pass);
        }
    }
}
