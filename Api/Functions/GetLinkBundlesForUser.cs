using Api.Utility;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class GetLinkBundlesForUser
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Hasher _hasher;
        private readonly IConfiguration _configuration;

        public GetLinkBundlesForUser(CosmosClient cosmosClient, Hasher hasher, IConfiguration configuration)
        {
            _cosmosClient = cosmosClient;
            _hasher = hasher;
            _configuration = configuration;
        }

        [Function(nameof(GetLinkBundlesForUser))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user")] HttpRequestData req)
        {
            try
            {
                var databaseName = _configuration["COSMOSDB_DATABASE"];
                var containerName = _configuration["COSMOSDB_CONTAINER"];

                var database = _cosmosClient.GetDatabase(databaseName);
                var container = database.GetContainer(containerName);

                var res = req.CreateResponse();

                ClientPrincipal clientPrincipal = ClientPrincipalUtility.GetClientPrincipal(req);

                if (clientPrincipal != null)
                {
                    string userDetails = clientPrincipal.UserDetails;
                    string username = _hasher.HashString(userDetails);
                    string provider = clientPrincipal.IdentityProvider;

                    var query = new QueryDefinition("SELECT c.id, c.vanityUrl, c.description, c.links FROM c WHERE c.userId = @username AND c.provider = @provider")
                        .WithParameter("@username", username)
                        .WithParameter("@provider", provider)
                        .WithParameter("@userDetails", userDetails);

                    var response = await container.GetItemQueryIterator<LinkBundle>(query).ReadNextAsync();

                    if (response.Count == 0)
                    {
                        return await req.CreateJsonResponse(HttpStatusCode.NotFound, "No link bundles found for user");
                    }

                    await res.WriteAsJsonAsync(response);

                    return res;
                }

                // return 401 if no client principal
                return await req.CreateJsonResponse(HttpStatusCode.Unauthorized, "Unauthorized");
            }
            catch (Exception ex)
            {
                return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
