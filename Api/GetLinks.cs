using System;
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
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "links/{vanityUrl}")] HttpRequestData req, [CosmosDBInput(databaseName: "linkylinkdb",
                           collectionName: "linkbundles",
                           ConnectionStringSetting = "CosmosDBConnectionString", 
                           SqlQuery = "SELECT * FROM c WHERE c.vanityUrl={vanityUrl}")] LinkBundle[] linkItems)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            Console.WriteLine("");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(linkItems);

            return response;
        }
    }
}
