using Api.Services;
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
    public class DeleteLinks
    {
        private readonly ILogger _logger;
        private readonly IDataService _dataService;

        public DeleteLinks(ILoggerFactory loggerFactory, IDataService dataService)
        {
            _logger = loggerFactory.CreateLogger<GetLinks>();
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        }

        [Function("DeleteLinks")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "links/{vanityUrl}")] HttpRequestData req,
            string vanityUrl)
        {
            var response = req.CreateResponse();

            if (string.IsNullOrEmpty(vanityUrl))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var linkBundle = await _dataService.GetLinkBundle(vanityUrl);

            if (linkBundle == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                return req.CreateResponse();
            }

            await _dataService.DeleteLinkBundle(linkBundle);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}