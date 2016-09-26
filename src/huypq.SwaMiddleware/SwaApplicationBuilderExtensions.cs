﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.DataProtection;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Net.Http.Headers;

namespace huypq.SwaMiddleware
{
    public static class SwaApplicationBuilderExtensions
    {
        private static string _controllerNamespacePattern;
        private static IApplicationBuilder _app;

        /// <summary>
        /// Extension methods for <see cref="IApplicationBuilder"/> to add SWA with Token Authentication to the request execution pipeline.
        /// All config can be set in singleton SwaSettings.Instance.
        /// Dependence services: Routing
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="applicationNamespace">the namespace of Startup class</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwaWithTokenAuthentication(
            this IApplicationBuilder app, string applicationNamespace)
        {
            SwaSettings.Instance.IsUseTokenAuthentication = true;
            SwaSettings.Instance.TokenEnpoint = "user.token";
            SwaSettings.Instance.AllowAnonymousActions = new List<string>() { "user.register" };

            return app.UseSwa(applicationNamespace);
        }

        /// <summary>
        /// Extension methods for <see cref="IApplicationBuilder"/> to add SWA (without authentication) to the request execution pipeline.
        /// All config can be set in singleton SwaSettings.Instance.
        /// Dependence services: Routing
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="applicationNamespace">the namespace of Startup class</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwa(
            this IApplicationBuilder app, string applicationNamespace)
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

            _app = app;

            var routeBuilder = new RouteBuilder(app, new RouteHandler(SwaRouteHandler));

            routeBuilder.MapRoute(
                name: "default",
                template: "{controller}/{action}");

            return app.UseRouter(routeBuilder.Build());
        }

        private static async Task SwaRouteHandler(HttpContext context)
        {
            try
            {
                var routeValues = context.GetRouteData().Values;
                var controller = routeValues["controller"]?.ToString().ToLower();
                var action = routeValues["action"]?.ToString().ToLower();

                var parameter = GetRequestParameter(context.Request);

                SwaActionResult result = RequestExecutor(controller, action, parameter, context.Request);

                if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    context.Response.StatusCode = (int)result.StatusCode;
                    return;
                }

                var responseType = "json";
                if (context.Request.Headers["response"].Count == 1)
                {
                    responseType = context.Request.Headers["response"][0];
                }

                await WriteResponse(context.Response, responseType, result);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                return;
            }
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

        private static SwaActionResult RequestExecutor(
            string controller, string action, Dictionary<string, object> parameter, HttpRequest request)
        {
            SwaActionResult result = null;

            if (SwaSettings.Instance.IsUseTokenAuthentication)
            {
                var protector = _app.ApplicationServices.GetDataProtector("token");
                string base64PlainToken = "";

                if (SwaSettings.Instance.TokenEnpoint == (controller + "." + action))
                {
                    result = ControllerInvoker(controller, action, parameter, null);
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        base64PlainToken = (result.ResultValue as SwaTokenModel).ToBase64();
                        result.ResultValue = protector.Protect(base64PlainToken);
                    }
                }
                else if (SwaSettings.Instance.AllowAnonymousActions.Contains(controller + "." + action))
                {
                    result = ControllerInvoker(controller, action, parameter, null);
                }
                else
                {
                    try
                    {
                        base64PlainToken = protector.Unprotect(request.Headers["token"][0]);
                        result = ControllerInvoker(
                            controller, action, parameter, SwaTokenModel.FromBase64(base64PlainToken));
                    }
                    catch (Exception ex)
                    {
                        result = new SwaActionResult
                        {
                            StatusCode = System.Net.HttpStatusCode.Unauthorized
                        };
                    }
                }
            }
            else
            {
                result = ControllerInvoker(controller, action, parameter, null);
            }

            return result;
        }

        private static SwaActionResult ControllerInvoker(
            string controllerName, string actionName, Dictionary<string, object> parameter, SwaTokenModel token)
        {
            var controllerType = Type.GetType(
                string.Format(_controllerNamespacePattern, controllerName), true, true);

            SwaController controller = Activator.CreateInstance(controllerType) as SwaController;
            controller.TokenModel = token;
            controller.App = _app;
            controller.Result = new SwaActionResult();

            return controller.ActionInvoker(actionName, parameter);
        }

        private static async Task WriteResponse(HttpResponse response, string responseType, SwaActionResult result)
        {
            if (result.ResultValue == null)
            {
                response.StatusCode = (int)result.StatusCode;
                return;
            }

            response.StatusCode = (int)result.StatusCode;
            response.ContentType = result.ContentType;
            switch (result.ResultType)
            {
                case SwaActionResult.ActionResultType.Json:
                    response.Headers["Content-Encoding"] = "gzip";
                    SwaSettings.Instance.JsonSerializer.Serialize(response.Body, result.ResultValue);
                    break;
                case SwaActionResult.ActionResultType.Object:
                    switch (responseType)
                    {
                        case "protobuf":
                            response.ContentType = "application/octet-stream";
                            SwaSettings.Instance.BinarySerializer.Serialize(response.Body, result.ResultValue);
                            break;
                        case "json":
                            response.Headers["Content-Encoding"] = "gzip";
                            response.ContentType = "application/json";
                            SwaSettings.Instance.JsonSerializer.Serialize(response.Body, result.ResultValue);
                            break;
                    }
                    break;
                case SwaActionResult.ActionResultType.Status:
                    response.ContentLength = 0;
                    break;
                case SwaActionResult.ActionResultType.Stream:
                    using (var stream = result.ResultValue as Stream)
                    {
                        response.ContentLength = stream.Length;
                        await stream.CopyToAsync(response.Body);
                    }
                    break;
                case SwaActionResult.ActionResultType.File:
                    var contentDisposition = new ContentDispositionHeaderValue("attachment");
                    contentDisposition.SetHttpFileName(result.FileName);
                    response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
                    using (var stream = result.ResultValue as Stream)
                    {
                        response.ContentLength = stream.Length;
                        await stream.CopyToAsync(response.Body);
                    }
                    break;                
            }
        }
    }
}
