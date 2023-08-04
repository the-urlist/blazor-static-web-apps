using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Shared
{
    public class Link
    {
        public string Id { get; set; }

        [Url]
        public string Url { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}