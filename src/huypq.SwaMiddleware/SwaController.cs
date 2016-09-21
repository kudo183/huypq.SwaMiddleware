using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.IO;

namespace huypq.SwaMiddleware
{
    public abstract class SwaController
    {
        /// <summary>
        /// result of the action
        /// </summary>
        public SwaActionResult Result { get; set; }

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
        /// response body is json text
        /// </summary>
        /// <param name="resultValue"></param>
        /// <returns></returns>
        protected SwaActionResult CreateJsonResult(
            object resultValue)
        {
            Result.ResultType = SwaActionResult.ActionResultType.Json;
            Result.ResultValue = resultValue;
            Result.ContentType = "application/json";
            Result.StatusCode = System.Net.HttpStatusCode.OK;
            return Result;
        }

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
            Result.ResultType = SwaActionResult.ActionResultType.Stream;
            Result.ResultValue = resultValue;
            Result.ContentType = mimeType;
            Result.StatusCode = System.Net.HttpStatusCode.OK;
            return Result;
        }

        /// <summary>
        /// response body is empty
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        protected SwaActionResult CreateStatusResult(
            System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.OK)
        {
            Result.ResultType = SwaActionResult.ActionResultType.Status;
            Result.ResultValue = null;
            Result.StatusCode = statusCode;
            return Result;
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
            Result.ResultType = SwaActionResult.ActionResultType.File;
            Result.ResultValue = resultValue;
            Result.ContentType = MimeMapping.GetMimeType(fileName);
            Result.FileName = fileName;
            Result.StatusCode = System.Net.HttpStatusCode.OK;
            return Result;
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
            Result.ResultType = SwaActionResult.ActionResultType.Object;
            Result.ResultValue = resultValue;
            Result.StatusCode = System.Net.HttpStatusCode.OK;
            return Result;
        }

    }
}
