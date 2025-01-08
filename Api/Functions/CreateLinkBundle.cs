using Api.Utility;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api.Functions
{
    public partial class CreateLinkBundle(CosmosClient cosmosClient, Hasher hasher, IConfiguration configuration)
    {
        protected const string CHARACTERS = "abcdefghijklmnopqrstuvwxyz0123456789";
        protected const string VANITY_REGEX = @"^([\w\d-])+(/([\w\d-])+)*$";

        [Function(nameof(CreateLinkBundle))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "links")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SaveLinks");
            logger.LogInformation("C# HTTP trigger function processed a request.");
            var linkBundle = await req.ReadFromJsonAsync<LinkBundle>();

            if (!ValidatePayLoad(linkBundle))
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Invalid payload");
            }

            EnsureVanityUrl(linkBundle);
            Match match = VanityRegex().Match(linkBundle.VanityUrl);

            if (!match.Success)
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Invalid vanity url");
            }

            ClientPrincipal clientPrincipal = ClientPrincipalUtility.GetClientPrincipal(req);

            if (clientPrincipal != null)
            {
                string username = clientPrincipal.UserDetails;
                linkBundle.UserId = hasher.HashString(username);
                linkBundle.Provider = clientPrincipal.IdentityProvider;
            }

            // Set the DateTime property of the LinkBundle to the current date and time
            linkBundle.CreatedDate = DateTime.UtcNow;

            // Set the referrer data of the LinkBundle to the referrer data from the request headers
            if (req.Headers.TryGetValues("Referer", out var referrerValues))
            {
                linkBundle.ReferrerData = referrerValues.FirstOrDefault();
            }

            try
            {
                var databaseName = configuration["COSMOSDB_DATABASE"];
                var containerName = configuration["COSMOSDB_CONTAINER"];

                var database = cosmosClient.GetDatabase(databaseName);
                var container = database.GetContainer(containerName);

                string vanityUrl = linkBundle.VanityUrl;
                var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vanityUrl = @vanityUrl").WithParameter("@vanityUrl", vanityUrl);

                var result = await container.GetItemQueryIterator<LinkBundle>(query).ReadNextAsync();

                var partitionKey = new PartitionKey(linkBundle.VanityUrl);

                var response = await container.CreateItemAsync(linkBundle);
                var responseMessage = req.CreateResponse(HttpStatusCode.Created);
                responseMessage.Headers.Add("Location", $"/{linkBundle.VanityUrl}");
                var linkDocument = response.Resource;
                await responseMessage.WriteAsJsonAsync(linkDocument);

                return responseMessage;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                return await req.CreateJsonResponse(HttpStatusCode.Conflict, "Vanity url already exists");
            }
            catch (Exception ex)
            {
                return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        private static void EnsureVanityUrl(LinkBundle linkDocument)
        {
            if (string.IsNullOrWhiteSpace(linkDocument.VanityUrl))
            {
                var random = new Random();
                var code = new string(Enumerable.Repeat(CHARACTERS, 7)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                linkDocument.VanityUrl = code;
            }
        }

        private static bool ValidatePayLoad(LinkBundle linkDocument)
        {
            return linkDocument != null && linkDocument.Links.Count > 0;
        }

        [GeneratedRegex(VANITY_REGEX, RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex VanityRegex();
    }
}
