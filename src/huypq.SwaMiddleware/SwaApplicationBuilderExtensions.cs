using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace huypq.SwaMiddleware
{
    public static class SwaApplicationBuilderExtensions
    {
        private static string _controllerNamespacePattern;
        private static JsonSerializer _jsonSerializer = JsonSerializer.Create();
        private static bool _isUseTokenAuthentication;
        private static IApplicationBuilder _app;
        private static List<string> _allowAnonymous;

        /// <summary>
        /// Extension methods for <see cref="IApplicationBuilder"/> to add SWA to the request execution pipeline.
        /// Dependence services: Routing
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="applicationNamespace">the namespace of Startup class</param>
        /// <param name="isUseTokenAuthentication">if true need to provide UserController with GetToken action.
        /// GetToken action will verify user, password and return a token string, which will be encrypt before sent to client
        /// </param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwa(
            this IApplicationBuilder app, string applicationNamespace, bool isUseTokenAuthentication, List<string> allowAnonymous)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (string.IsNullOrEmpty(applicationNamespace))
            {
                throw new ArgumentNullException(nameof(applicationNamespace));
            }

            var entryAssembly = Assembly.Load(new AssemblyName(applicationNamespace));

            _controllerNamespacePattern = string.Format("{0}.Controllers.{{0}}Controller, {1}",
                entryAssembly.FullName.Split(',')[0], entryAssembly.FullName);
            _isUseTokenAuthentication = isUseTokenAuthentication;
            _app = app;
            _allowAnonymous = allowAnonymous;

            var routeBuilder = new RouteBuilder(app, new RouteHandler(SwaRouteHandler));

            routeBuilder.MapRoute(
                name: "default",
                template: "{controller}/{action}");

            return app.UseRouter(routeBuilder.Build());
        }

        private static Task SwaRouteHandler(HttpContext context)
        {
            var routeValues = context.GetRouteData().Values;
            var controller = routeValues["controller"]?.ToString().ToLower();
            var action = routeValues["action"]?.ToString().ToLower();

            var parameter = GetRequestParameter(context.Request);

            object result = null;

            if (_isUseTokenAuthentication)
            {
                var protector = _app.ApplicationServices.GetDataProtector("token");
                string base64Token = "";

                if (controller == "user" && action == "token")
                {
                    base64Token = (ControllerInvoker(controller, action, parameter, null) as SwaTokenModel).ToBase64();
                    result = protector.Protect(base64Token);
                }
                else if (_allowAnonymous.Contains(controller + "." + action))
                {
                    result = ControllerInvoker(controller, action, parameter, null);
                }
                else
                {
                    try
                    {
                        base64Token = protector.Unprotect(context.Request.Headers["token"][0]);
                        result = ControllerInvoker(
                            controller, action, parameter, SwaTokenModel.FromBase64(base64Token));
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                        return Task.FromResult(0);
                    }
                }
            }
            else
            {
                result = ControllerInvoker(controller, action, parameter, null);
            }

            context.Response.ContentType = "application/json";

            return JsonWrite(context.Response.Body, result);
        }

        private static Dictionary<string, object> GetRequestParameter(HttpRequest request)
        {
            var parameter = new Dictionary<string, object>();

            foreach (var q in request.Query)
            {
                parameter.Add(q.Key, q.Value);
            }

            if (request.HasFormContentType)
            {
                foreach (var f in request.Form)
                {
                    parameter.Add(f.Key, f.Value);
                }

                foreach (var file in request.Form.Files)
                {
                    parameter.Add(file.Name, file);
                }
            }

            return parameter;
        }

        private static object ControllerInvoker(
            string controllerName, string actionName, Dictionary<string, object> parameter, SwaTokenModel token)
        {
            object result = null;

            var controllerType = Type.GetType(
                string.Format(_controllerNamespacePattern, controllerName), true, true);
            SwaController controller = Activator.CreateInstance(controllerType) as SwaController;
            controller.TokenModel = token;
            controller.App = _app;
            result = controller.ActionInvoker(actionName, parameter);

            return result;
        }

        private static async Task JsonWrite(System.IO.Stream body, object value)
        {
            using (var writer = new System.IO.StreamWriter(body))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                _jsonSerializer.Serialize(jsonWriter, value);
                await writer.FlushAsync();
            }
        }
    }
}
