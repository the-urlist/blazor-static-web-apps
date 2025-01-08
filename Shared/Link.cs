using System;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class Link
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("image")]
        public string Image { get; set; }
        [JsonPropertyName("clicks")]
        public int Clicks { get; set; }
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; }
    }
}
