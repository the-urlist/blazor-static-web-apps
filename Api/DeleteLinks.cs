using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api
{
    public class DeleteLinks
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;

        public DeleteLinks(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
        {
            _logger = loggerFactory.CreateLogger<DeleteLinks>();
            _cosmosClient = cosmosClient;
        }

        [Function("DeleteLinks")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "links/{vanityUrl}")] HttpRequestData req,
            string vanityUrl)
        {
            var container = _cosmosClient.GetContainer("TheUrlist", "linkbundles");

            var link = container.GetItemLinqQueryable<LinkBundle>(true)
                                 .Where(l => l.VanityUrl == vanityUrl)
                                 .ToList()
                                 .FirstOrDefault();

            if (link == null)
            {
                var response = req.CreateResponse();
                await response.WriteStringAsync("Link not found");
                response.StatusCode = System.Net.HttpStatusCode.NotFound;
                return response;
            }
            else
            {
                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

                await container.DeleteItemAsync<LinkBundle>(link.id, new PartitionKey(link.VanityUrl));

                await response.WriteStringAsync("Link deleted");
                return response;
            }
        }
    }
}