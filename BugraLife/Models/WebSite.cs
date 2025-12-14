using System.ComponentModel.DataAnnotations;

namespace BugraLife.Models
{
    public class WebSite
    {
        [Key]
        public int website_id { get; set; }
        public string? website_name { get; set; }
        public string? website_url { get; set; }
        public string? website_description { get; set; }

    }
}
