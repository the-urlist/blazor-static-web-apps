using Api.Services;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class SaveLinks
    {
        protected const string CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        protected const string VANITY_REGEX = @"^([\w\d-])+(/([\w\d-])+)*$";

        private readonly ILogger _logger;
        private readonly IDataService _dataService;

        public SaveLinks(ILoggerFactory loggerFactory, IDataService dataService)
        {
            _logger = loggerFactory.CreateLogger<SaveLinks>();
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        }

        [Function("SaveLinks")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "links")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var body = await req.ReadAsStringAsync();
            var linkBundle = System.Text.Json.JsonSerializer.Deserialize<LinkBundle>(body);

            if (!ValidatePayLoad(linkBundle, req))
            {
                var res = req.CreateResponse(HttpStatusCode.BadRequest);
                await res.WriteAsJsonAsync(new { error = "Invalid payload" });
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
                await _dataService.SaveLinkBundle(linkBundle);
                var responseMessage = req.CreateResponse(HttpStatusCode.Created);
                responseMessage.Headers.Add("Location", $"/{linkBundle.VanityUrl}");
                await responseMessage.WriteAsJsonAsync(linkBundle);
                return responseMessage;
            }
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

            var linkBundle = await req.ReadFromJsonAsync<LinkBundle>();

            if (!ValidatePayLoad(linkBundle, req))
            {
                var res = req.CreateResponse(HttpStatusCode.BadRequest);
                await res.WriteAsJsonAsync(new { error = "Invalid payload" });
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
                await _dataService.UpdateLinkBundle(linkBundle);
                var responseMessage = req.CreateResponse(HttpStatusCode.OK);
                await responseMessage.WriteAsJsonAsync(linkBundle);
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

            linkDocument.VanityUrl = linkDocument.VanityUrl.ToLower();
        }

        private static bool ValidatePayLoad(LinkBundle linkDocument, HttpRequestData req)
        {
            bool isValid = linkDocument != null && linkDocument.Links.Count() > 0;
            return isValid;
        }
    }
}