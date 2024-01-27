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
using Api.Utility;

namespace Api
{
    public class UpdateLinkBundle
    {
        protected const string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        protected const string VANITY_REGEX = @"^([\w\d-])+(/([\w\d-])+)*$";

        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;

        public UpdateLinkBundle(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
        {
            _logger = loggerFactory.CreateLogger<UpdateLinkBundle>();
            _cosmosClient = cosmosClient;
        }

        [Function(nameof(UpdateLinkBundle))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "links/{vanityUrl}")] HttpRequestData req,
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
                var container = _cosmosClient.GetContainer("TheUrlist", "linkbundles");
                var partitionKey = new PartitionKey(vanityUrl);
                var response = await container.UpsertItemAsync(linkBundle, partitionKey);
                var responseMessage = req.CreateResponse(HttpStatusCode.OK);
                var linkDocument = response.Resource;
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
