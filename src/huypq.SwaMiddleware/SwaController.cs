using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.IO;

namespace huypq.SwaMiddleware
{
    public abstract class SwaController
    {
        /// <summary>
        /// request paramter type: json or protobuf
        /// </summary>
        public string RequestObjectType { get; set; }
        
        /// <summary>
        /// authentication token
        /// </summary>
        public SwaTokenModel TokenModel { get; set; }

        /// <summary>
        /// application builder, use for get Dependency Injection service
        /// </summary>
        public IApplicationBuilder App { get; set; }

        /// <summary>
        /// mapping actionName to corresponding controller method, including paremeter convert.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public abstract SwaActionResult ActionInvoker(string actionName, Dictionary<string, object> parameter);
        
        /// <summary>
        /// response body is binary stream
        /// </summary>
        /// <param name="resultValue"></param>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        protected SwaActionResult CreateStreamResult(
            Stream resultValue,
            string mimeType)
        {
            var result = new SwaActionResult();
            result.ResultType = SwaActionResult.ActionResultType.Stream;
            result.ResultValue = resultValue;
            result.ContentType = mimeType;
            result.StatusCode = System.Net.HttpStatusCode.OK;
            return result;
        }

        /// <summary>
        /// response body is empty
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        protected SwaActionResult CreateStatusResult(
            System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.OK)
        {
            var result = new SwaActionResult();
            result.ResultType = SwaActionResult.ActionResultType.Status;
            result.ResultValue = null;
            result.StatusCode = statusCode;
            return result;
        }

        /// <summary>
        /// response body is binary stream
        /// </summary>
        /// <param name="resultValue"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected SwaActionResult CreateFileResult(
            Stream resultValue,
            string fileName)
        {
            var result = new SwaActionResult();
            result.ResultType = SwaActionResult.ActionResultType.File;
            result.ResultValue = resultValue;
            result.ContentType = MimeMapping.GetMimeType(fileName);
            result.FileName = fileName;
            result.StatusCode = System.Net.HttpStatusCode.OK;
            return result;
        }

        /// <summary>
        /// response type is chosen by client request Headers["response"]:
        ///     "json" -> json text
        ///     "protobuf" -> protobuf binary
        /// </summary>
        /// <param name="resultValue"></param>
        /// <returns></returns>
        protected SwaActionResult CreateObjectResult(
            object resultValue)
        {
            var result = new SwaActionResult();
            result.ResultType = SwaActionResult.ActionResultType.Object;
            result.ResultValue = resultValue;
            result.StatusCode = System.Net.HttpStatusCode.OK;
            return result;
        }

    }
}
