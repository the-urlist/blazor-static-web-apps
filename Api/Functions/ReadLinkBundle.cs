using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class ReadLinkBundle
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;

        public ReadLinkBundle(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
        {
            _logger = loggerFactory.CreateLogger<ReadLinkBundle>();
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        }

        [Function("GetLinks")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "links/{vanityUrl}")] HttpRequestData req, string vanityUrl)
        {
            if (string.IsNullOrEmpty(vanityUrl))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var database = _cosmosClient.GetDatabase("TheUrlist");
            var container = database.GetContainer("linkbundles");

            var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vanityUrl = @vanityUrl")
                            .WithParameter("@vanityUrl", vanityUrl);

            var result = await container.GetItemQueryIterator<LinkBundle>(query).ReadNextAsync();

            if (!result.Any())
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result.First());
            return response;
        }
    }
}