using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Shared
{
    public class Link
    {
        public string Id { get; set; }

        [RegularExpression(@"^(https?://)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$", ErrorMessage = "That doesn't look like a valid URL"), Required]
        public string Url { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}