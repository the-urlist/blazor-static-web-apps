using Api.Models;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api
{
    public class SaveLinks
    {
        private readonly ILogger _logger;

        public SaveLinks(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SaveLinks>();
        }

        [Function("SaveLinks")]
        public async Task<SaveLinkResponse> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "links")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("HttpExample");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var message = "Welcome to Azure Functions!";

            // Deserialize JSON from request body into a LinkBundle object.
            var linkBundle = await req.ReadFromJsonAsync<LinkBundle>();

            if (!ValidatePayLoad(linkBundle, req))
            {
                _logger.LogError("Link validation failed");
                return new SaveLinkResponse()
                {
                    NewLinkBundle = null,
                    HttpResponse = req.CreateResponse(HttpStatusCode.BadRequest)
                };
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(message);

            // Return a response to both HTTP trigger and Azure Cosmos DB output binding.
            return new SaveLinkResponse()
            {
                NewLinkBundle = linkBundle,
                HttpResponse = response
            };
        }

        private static bool ValidatePayLoad(LinkBundle linkDocument, HttpRequestData req)
        {
            bool isValid = (linkDocument != null) && linkDocument.Links.Count() > 0;

            return isValid;
        }
    }
}
