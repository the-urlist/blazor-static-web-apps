using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api
{
    public class SaveLinks
    {
        protected const string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        protected const string VANITY_REGEX = @"^([\w\d-])+(/([\w\d-])+)*$";

        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;

        public SaveLinks(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
        {
            _logger = loggerFactory.CreateLogger<SaveLinks>();
            _cosmosClient = cosmosClient;
        }

        [Function("SaveLinks")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "links")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SaveLinks");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            // Deserialize JSON from request body into a LinkBundle object.
            var linkBundle = await req.ReadFromJsonAsync<LinkBundle>();

            if (!ValidatePayLoad(linkBundle, req))
            {
                var res = req.CreateResponse(HttpStatusCode.BadRequest);
                await res.WriteAsJsonAsync(new { error = "Invalid payload" });

                // Return the response
                return res;
            }

            EnsureVanityUrl(linkBundle);

            Match match = Regex.Match(linkBundle.VanityUrl, VANITY_REGEX, RegexOptions.IgnoreCase);

            ClientPrincipal principal = null;

            if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
            {
                var data = header.FirstOrDefault();
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if(principal != null)
                {
                    string username = principal.UserDetails;
                    // Hash the username using the Hasher class
                    Hasher hasher = new Hasher();
                    linkBundle.UserId = hasher.HashString(username);
                    linkBundle.Provider  = principal.IdentityProvider;
                }
            }

            if (!match.Success)
            {
                var res = req.CreateResponse(HttpStatusCode.BadRequest);
                await res.WriteAsJsonAsync(new { error = "Invalid vanity url" });

                return res;
            }

            try
            {
                // Get the cosmos container
                var container = _cosmosClient.GetContainer("TheUrlist", "linkbundles");

                // Create the document
                var partitionKey = new PartitionKey(linkBundle.VanityUrl);
                var response = await container.CreateItemAsync(linkBundle);

                // Return the response
                var responseMessage = req.CreateResponse(HttpStatusCode.Created);

                // Add the location header
                responseMessage.Headers.Add("Location", $"/{linkBundle.VanityUrl}");

                // Get the document from response
                var linkDocument = response.Resource;

                // Write the document to the response
                await responseMessage.WriteAsJsonAsync(linkDocument);

                return responseMessage;
            }
            // catch specific exception for document conflict
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                var res = req.CreateResponse();
                await res.WriteAsJsonAsync(new { error = "Vanity url already exists" }, HttpStatusCode.Conflict);

                return res;
            }
            catch (Exception ex)
            {
                var res = req.CreateResponse();
                await res.WriteAsJsonAsync(new { error = ex.Message }, HttpStatusCode.InternalServerError);

                return res;
            }

        }

        [Function("UpdateLinks")]
        public async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "links/{vanityUrl}")] HttpRequestData req,
                       string vanityUrl, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateLinks");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            // Deserialize JSON from request body into a LinkBundle object.
            var linkBundle = await req.ReadFromJsonAsync<LinkBundle>();

            if (!ValidatePayLoad(linkBundle, req))
            {
                var res = req.CreateResponse(HttpStatusCode.BadRequest);
                await res.WriteAsJsonAsync(new { error = "Invalid payload" });

                // Return the response
                return res;
            }

            if (string.IsNullOrEmpty(vanityUrl))
            {
                var res = req.CreateResponse(HttpStatusCode.BadRequest);
                await res.WriteAsJsonAsync(new { error = "Invalid vanity url" });

                return res;
            }

            try
            {
                // Get the cosmos container
                var container = _cosmosClient.GetContainer("TheUrlist", "linkbundles");

                // Create the document
                var partitionKey = new PartitionKey(vanityUrl);
                var response = await container.UpsertItemAsync(linkBundle, partitionKey);

                // Return the response
                var responseMessage = req.CreateResponse(HttpStatusCode.OK);

                // Get the document from response
                var linkDocument = response.Resource;

                // Write the document to the response
                await responseMessage.WriteAsJsonAsync(linkDocument);

                return responseMessage;
            }
            catch (Exception ex)
            {
                var res = req.CreateResponse();
                await res.WriteAsJsonAsync(new { error = ex.Message }, HttpStatusCode.InternalServerError);

                return res;
            }

        }   

        private void EnsureVanityUrl(LinkBundle linkDocument)
        {
            if (string.IsNullOrWhiteSpace(linkDocument.VanityUrl))
            {
                var random = new Random();
                var code = new string(Enumerable.Repeat(CHARACTERS, 7)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                linkDocument.VanityUrl = code;
            }

            // force lowercase
            linkDocument.VanityUrl = linkDocument.VanityUrl.ToLower();
        }

        private static bool ValidatePayLoad(LinkBundle linkDocument, HttpRequestData req)
        {
            bool isValid = (linkDocument != null) && linkDocument.Links.Count() > 0;

            return isValid;
        }
    }
}
