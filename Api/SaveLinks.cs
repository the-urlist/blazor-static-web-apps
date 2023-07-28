using Api.Models;
using BlazorApp.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public SaveLinks(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SaveLinks>();
        }

        [Function("SaveLinks")]
        public async Task<SaveLinkResponse> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "links")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SaveLinks");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            // Deserialize JSON from request body into a LinkBundle object.
            var linkBundle = await req.ReadFromJsonAsync<LinkBundle>();

            if (!ValidatePayLoad(linkBundle, req))
            {
                _logger.LogError("Link validation failed");
                return new SaveLinkResponse()
                {
                    NewLinkBundle = null,
                    HttpResponse = req.CreateResponse(HttpStatusCode.BadRequest)
                };
            }

            EnsureVanityUrl(linkBundle);

            Match match = Regex.Match(linkBundle.VanityUrl, VANITY_REGEX, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                // does not match
                _logger.LogError("Invalid vanity url");
                return new SaveLinkResponse()
                {
                    NewLinkBundle = null,
                    HttpResponse = req.CreateResponse(HttpStatusCode.BadRequest)
                };
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync<LinkBundle>(linkBundle);

            // Return a response to both HTTP trigger and Azure Cosmos DB output binding.
            return new SaveLinkResponse()
            {
                NewLinkBundle = linkBundle,
                HttpResponse = response
            };
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
