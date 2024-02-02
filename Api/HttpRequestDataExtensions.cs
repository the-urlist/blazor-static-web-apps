using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Threading.Tasks;

namespace Api
{
    public static class HttpRequestDataExtensions
    {
        public static async Task<HttpResponseData> CreateJsonResponse<T>(this HttpRequestData req, HttpStatusCode statusCode, T message)
        {
            var response = req.CreateResponse(statusCode);
            await response.WriteAsJsonAsync(new { error = message }, statusCode);
            return response;
        }

        public static async Task<HttpResponseData> CreateOkResponse<T>(this HttpRequestData req, T message)
        {
            return await req.CreateJsonResponse(HttpStatusCode.OK, message);
        }
    }
}
