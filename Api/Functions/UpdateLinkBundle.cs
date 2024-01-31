using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api
{
    public class UpdateLinkBundle(CosmosClient _cosmosClient)
    {
        [Function(nameof(UpdateLinkBundle))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "links/{vanityUrl}")] HttpRequestData req,
                       string vanityUrl, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateLinks");
            logger.LogInformation("C# HTTP trigger function processed a request.");
            var linkBundle = await req.ReadFromJsonAsync<LinkBundle>();

            if (!ValidatePayLoad(linkBundle))
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Invalid payload");
            }

            if (string.IsNullOrEmpty(vanityUrl))
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Invalid vanity url");
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
                return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static bool ValidatePayLoad(LinkBundle linkDocument)
        {
            return (linkDocument != null) && linkDocument.Links.Count > 0;
        }
    }
}
