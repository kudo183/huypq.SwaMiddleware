using Newtonsoft.Json;
using System.Collections.Generic;

namespace huypq.SwaMiddleware
{
    public class SwaOptions
    {
        /// <summary>
        /// If true: need set TokenEnpoint. Default all action need provider token.
        /// If want to allow anonymous access for some action, add to AllowAnonymousActions list (format:  {controller}.{action})
        /// If false: can skip TokenEnpoint and AllowAnonymousActions
        /// </summary>
        public bool IsUseTokenAuthentication { get; set; }
        /// <summary>
        /// Specify action which return token base on user, password.
        /// Format: {controller}.{action}
        /// </summary>
        public string TokenEnpoint { get; set; }
        /// <summary>
        /// Specify list of action which allow anonymous user
        /// </summary>
        public List<string> AllowAnonymousActions { get; set; }

        public JsonSerializer JsonSerializer { get; set; }
    }
}
