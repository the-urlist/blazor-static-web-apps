using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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
                var res = req.CreateResponse(HttpStatusCode.Conflict);
                await res.WriteAsJsonAsync(new { error = "Vanity url already exists" });

                return res;
            }
            catch (Exception ex)
            {
                var res = req.CreateResponse(HttpStatusCode.InternalServerError);
                await res.WriteAsJsonAsync(new { error = ex.Message });

                return res;
            }

        }

        private void EnsureVanityUrl(LinkBundle linkDocument)
        {
            if (string.IsNullOrWhiteSpace(linkDocument.VanityUrl))
            {
                var code = new char[7];
                var rng = new RNGCryptoServiceProvider();

                var bytes = new byte[sizeof(uint)];
                for (int i = 0; i < code.Length; i++)
                {
                    rng.GetBytes(bytes);
                    uint num = BitConverter.ToUInt32(bytes, 0) % (uint)CHARACTERS.Length;
                    code[i] = CHARACTERS[(int)num];
                }

                linkDocument.VanityUrl = new String(code);
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
