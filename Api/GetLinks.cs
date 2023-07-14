using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;


namespace ApiIsolated
{
    public class GetLinks
    {
        private readonly ILogger _logger;

        public GetLinks(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetLinks>();
        }

        [Function("GetLinks")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "links/{vanityUrl}")] 
                    HttpRequestData req, 
                    [CosmosDBInput(databaseName: "linkylinkdb",
                           collectionName: "linkbundles",
                           ConnectionStringSetting = "CosmosDBConnectionString",
                           SqlQuery = "SELECT TOP 1 * FROM c WHERE c.vanityUrl={vanityUrl}")] IEnumerable<LinkBundle> documents)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(documents.Single());

            return response;
        }
    }
}
