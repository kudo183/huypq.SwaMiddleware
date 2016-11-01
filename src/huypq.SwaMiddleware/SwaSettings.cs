using System.Collections.Generic;

namespace huypq.SwaMiddleware
{
    public sealed class SwaSettings
    {
        private static readonly SwaSettings _instance = new SwaSettings()
        {
            DefaultPageSize = 20,
            MaxItemAllowed = 1000,
            JsonSerializer = new SwaNewtonJsonSerializer(),
            BinarySerializer = new SwaProtobufBinarySerializer()
        };

        public static SwaSettings Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Result paging size
        /// </summary>
        public int DefaultPageSize { get; set; }

        /// <summary>
        /// Result paging size
        /// </summary>
        public int MaxItemAllowed { get; set; }

        /// <summary>
        /// If true: need set TokenEnpoint. Default all action need provide token.
        /// If want to allow anonymous access for some action, add to AllowAnonymousActions list (format:  {controller}.{action})
        /// If false: can skip TokenEnpoint and AllowAnonymousActions
        /// </summary>
        public bool IsUseTokenAuthentication { get; set; }

        /// <summary>
        /// Specify action which will verify user, password and return a encrypted token string.
        /// Format: {controller}.{action}
        /// Default is "user.token"
        /// </summary>
        public string TokenEnpoint { get; set; }

        /// <summary>
        /// Specify list of action which allow anonymous user
        /// Default is contain "user.register" for register acion
        /// </summary>
        public List<string> AllowAnonymousActions { get; set; }

        /// <summary>
        /// Use for deserialize request parameter and serialize response if Header["response"]="json"
        /// </summary>
        public SwaISerializer JsonSerializer { get; set; }

        /// <summary>
        /// Use for serialize response if Header["response"]="protobuf"
        /// </summary>
        public SwaISerializer BinarySerializer { get; set; }
    }
}
