using Api.Utility;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
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
            var response = req.CreateResponse();

            ClientPrincipal principal = ClientPrincipalUtility.GetClientPrincipal(req);

            var container = _cosmosClient.GetContainer("TheUrlist", "linkbundles");

            // get the document id where vanityUrl == vanityUrl
            var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vanityUrl = @vanityUrl")
                .WithParameter("@vanityUrl", vanityUrl);

            var result = await container.GetItemQueryIterator<LinkBundle>(query).ReadNextAsync();

            if (result.Any())
            {
                Hasher hasher = new Hasher();
                var hashedUsername = hasher.HashString(principal.UserDetails);
                if (hashedUsername != result.First().UserId || principal.IdentityProvider != result.First().Provider)
                {
                    await response.WriteStringAsync("Unauthorized");
                    response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                    return response;
                }

                var partitionKey = new PartitionKey(vanityUrl);
                await container.DeleteItemAsync<LinkBundle>(result.First().Id, partitionKey);

                await response.WriteStringAsync("Link deleted");
                return response;
            }


            await response.WriteStringAsync("Link not found");
            response.StatusCode = System.Net.HttpStatusCode.NotFound;
            return response;
        }
    }
}