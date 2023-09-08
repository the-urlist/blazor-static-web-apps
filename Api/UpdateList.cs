using BlazorApp.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api
{
    public class UpdateList
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger _logger;

        public UpdateList(CosmosClient cosmosClient, ILoggerFactory loggerFactory)
        {
            _cosmosClient = cosmosClient;
            _logger = loggerFactory.CreateLogger("LinkOperations");
        }

        [Function(nameof(UpdateList))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "PATCH", Route = "links/{vanityUrl}")] HttpRequest req,
                        string vanityUrl)
        {

            if (string.IsNullOrEmpty(vanityUrl))
            {
                return new BadRequestResult();
            }

            var container = _cosmosClient.GetContainer("TheUrlist", "linkbundles");

            var linkBundle = container.GetItemLinqQueryable<LinkBundle>(true)
                                 .Where(l => l.VanityUrl == vanityUrl)
                                 .ToList()
                                 .FirstOrDefault();

            if (linkBundle == null)
            {
                _logger.LogInformation($"Bundle for {vanityUrl} not found.");
                return new NotFoundResult();
            }

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                if (string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogError("Request body is empty.");
                    return new BadRequestResult();
                }

                JsonPatchDocument<LinkBundle> patchDocument = JsonConvert.DeserializeObject<JsonPatchDocument<LinkBundle>>(requestBody);

                if (!patchDocument.Operations.Any())
                {
                    _logger.LogError("Request body contained no operations.");
                    return new NoContentResult();
                }

                patchDocument.ApplyTo(linkBundle);

                Uri collUri = UriFactory.CreateDocumentCollectionUri("linkylinkdb", "linkbundles");
                await container.UpsertItemAsync(collUri, new PartitionKey(linkBundle.VanityUrl));
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, ex.Message);
                return new BadRequestResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new NoContentResult();
        }
    }
}