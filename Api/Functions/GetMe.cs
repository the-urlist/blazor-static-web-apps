using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Api.Utility;

namespace Api.Functions
{

    /* This endpoint mocks the /.auth/me endpoint in Azure Functions. This allows us to test auth locally with the SWA emulator. 
       It's necessary because the structure of the client principal in Static Web Apps is different than in Azure Functions.
       
    */
    public class GetMe(ILoggerFactory loggerFactory)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<GetMe>();

        [Function(nameof(GetMe))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me")] HttpRequestData req, string vanityUrl)
        {
            var clientPrincipal = ClientPrincipalUtility.GetClientPrincipal(req);

            var res = req.CreateResponse();
            await res.WriteAsJsonAsync(clientPrincipal);

            return res;
        }
    }
}
