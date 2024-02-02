using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class GetOpenGraphInfo
    {
        [Function(nameof(GetOpenGraphInfo))]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "oginfo")] HttpRequestData req)
        {
            var link = await req.ReadFromJsonAsync<Link>();
            if (!link.Url.StartsWith("http://") && !link.Url.StartsWith("https://"))
            {
                link.Url = $"https://{link.Url}";
            }

            var httpClient = new System.Net.Http.HttpClient();

            // add a header to mimic a browser
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.2210.77");

            var response = await httpClient.GetAsync(link.Url);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Unable to load URL");
            }

            var html = await response.Content.ReadAsStringAsync();
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var document = parser.ParseDocument(html);
            if (document == null)
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Unable to parse document");
            }

            var title = document.QuerySelector("title")?.TextContent
                        ?? document.QuerySelector("meta[property='og:title']")?.GetAttribute("content")
                        ?? document.QuerySelector("meta[property='og:site_name']")?.GetAttribute("content")
                        ?? "";

            var description = document.QuerySelector("meta[property='og:description']")?.GetAttribute("content")
                              ?? document.QuerySelector("meta[name='description']")?.GetAttribute("content")
                              ?? "";

            var image = document.QuerySelector("meta[property='og:image']")?.GetAttribute("content")
                         ?? document.QuerySelector("link[rel='apple-touch-icon']")?.GetAttribute("href")
                         ?? document.QuerySelector("link[rel='mask-icon']")?.GetAttribute("href")
                         ?? document.QuerySelector("link[rel='shortcut icon']")?.GetAttribute("href")
                         ?? "";

            // Update the link with the new information
            link.Title = title;
            link.Description = description;
            link.Image = image;

            // Return the updated link
            return await req.CreateOkResponse(link);
        }
    }
}
