using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api.Models
{
    public class SaveLinkResponse
    {
        [CosmosDBOutput(
            databaseName: "TheUrlist",
            collectionName: "linkbundles",
            ConnectionStringSetting = "CosmosDBConnectionString")]
        public LinkBundle? NewLinkBundle { get; set; }
        public HttpResponseData? HttpResponse { get; set; }
    }
}