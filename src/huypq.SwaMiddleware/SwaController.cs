using System.Collections.Generic;

namespace huypq.SwaMiddleware
{
    public abstract class SwaController
    {
        public SwaTokenModel TokenModel { get; set; }
        public abstract object ActionInvoker(string actionName, Dictionary<string, object> parameter);
    }
}
