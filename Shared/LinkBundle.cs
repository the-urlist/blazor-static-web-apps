using System.Collections.Generic;

namespace BlazorApp.Shared
{
    public class LinkBundle {
        public string Id { get; set; }
        public string VanityUrl { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }

        public List<Link> Links { get; set; } = new List<Link>();
    }
}