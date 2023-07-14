using System.Net;
using Api.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using BlazorApp.Shared;
using System.Collections.Generic;

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
        public SaveLinkResponse Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "links")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("HttpExample");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var message = "Welcome to Azure Functions!";

            // Deserialize JSON from request body into a LinkBundle object.
            var linkBundle = req.ReadFromJsonAsync<LinkBundle>();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString(message);

            // Return a response to both HTTP trigger and Azure Cosmos DB output binding.
            return new SaveLinkResponse()
            {
                NewLinkBundle = linkBundle.Result,
                HttpResponse = response
            };
        }
    }
}
