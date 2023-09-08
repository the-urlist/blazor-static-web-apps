using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class LinkBundle {
        [JsonPropertyName("id")]
        public string id { get; init; }
        [JsonPropertyName("vanityUrl")]
        public string VanityUrl { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("links")]
        public List<Link> Links { get; set; } = new List<Link>();
    }
}