using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HandlebarsDotNet;
using System.Linq;
using Net.Codecrete.QrCodeGenerator;

namespace Api.Functions
{
    public class GetPublicPage(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<ReadLinkBundle>();
        private readonly CosmosClient _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));

        [Function(nameof(GetPublicPage))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "public/{vanityUrl}")] HttpRequestData req, string vanityUrl)
        {
            if (string.IsNullOrEmpty(vanityUrl))
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "vanityUrl is required");
            }

            var database = _cosmosClient.GetDatabase("TheUrlist");
            var container = database.GetContainer("linkbundles");

            var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vanityUrl = @vanityUrl")
                            .WithParameter("@vanityUrl", vanityUrl);

            var result = await container.GetItemQueryIterator<LinkBundle>(query).ReadNextAsync();

            if (result.Count == 0)
            {
                // TODO: Link Bundle Not Found
                return await req.CreateJsonResponse(HttpStatusCode.NotFound, "No LinkBundle found for this vanity url");
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            LinkBundle linkBundle = result.First();

            string source;
            using (var reader = new StreamReader("../../Templates/public.html"))
            {
                source = await reader.ReadToEndAsync();
            }

            // Decode the vanityUrl from base64
            // We're using base64 encoding to allow for special characters in the vanityUrl
            // string decodedVanityUrl = Encoding.UTF8.GetString(Convert.FromBase64String(vanityUrl));
            var qrCode = QrCode.EncodeText($"https://theurlist.com/{vanityUrl}", QrCode.Ecc.Medium);

            var data = new
            {
                linkBundle,
                qrCode = qrCode.ToSvgString(4, "#121212", "#F9FAFC")
            };

            var template = Handlebars.Compile(source);
            var rendered = template(data);

            response.WriteString(rendered);

            return response;
        }
    }
}
