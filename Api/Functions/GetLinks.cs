using Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class GetLinks
    {
        private readonly ILogger _logger;
        private readonly IDataService _dataService;

        public GetLinks(ILoggerFactory loggerFactory, IDataService dataService)
        {
            _logger = loggerFactory.CreateLogger<GetLinks>();
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        }

        [Function("GetLinks")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "links/{vanityUrl}")] HttpRequestData req, string vanityUrl)
        {
            if (string.IsNullOrEmpty(vanityUrl))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var linkBundle = await _dataService.GetLinkBundle(vanityUrl);

            if (linkBundle == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(linkBundle);
            return response;
        }
    }
}