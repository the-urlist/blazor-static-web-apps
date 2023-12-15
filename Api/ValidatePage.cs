using AngleSharp.Html.Parser;
using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api
{
    public class ValidatePage
    {
        private readonly ILogger _logger;

        public ValidatePage(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SaveLinks>();
        }

        [Function("ValidatePage")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "validatePage")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ValidatePage");
            var link = await req.ReadFromJsonAsync<Link>();

            // 1. Create an HTTP Client
            // 2. Make a request to the URL
            // 3. Parse the response using AngleSharp
            // 4. Read the title, description, and image

            var client = new HttpClient();

            // check if the URL starts with http:// or https:// and if not, add https://
            if (!link.Url.StartsWith("http://") && !link.Url.StartsWith("https://"))
            {
                link.Url = $"https://{link.Url}";
            }
            var response = await client.GetAsync(link.Url);
            var content = await response.Content.ReadAsStringAsync();

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(content);

            var title = document.QuerySelector("title")?.TextContent;
            if (string.IsNullOrEmpty(title))
            {
                title = document.QuerySelector("meta[property='og:title']")?.GetAttribute("content");
            }
            var description = document.QuerySelector("meta[property='og:description']")?.GetAttribute("content");
            var image = document.QuerySelector("meta[property='og:image']")?.GetAttribute("content");

            link.Title = title;
            link.Description = description;
            link.Image = image;

            // Return the link in JSON format
            var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await res.WriteAsJsonAsync(link);
            return res;
        }
    }
}
