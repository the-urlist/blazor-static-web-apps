using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System;

namespace BlazorApp.Shared
{
    public class LinkBundle
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonPropertyName("vanityUrl")]
        public string VanityUrl { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("provider")]
        public string Provider { get; set; }
        [JsonPropertyName("links")]
        public List<Link> Links { get; set; } = new List<Link>();
    }
}