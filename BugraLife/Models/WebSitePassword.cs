using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugraLife.Models
{
    public class WebSitePassword
    {
        [Key]
        public int websitepassword_id { get; set; }

        public int website_id { get; set; }

        [ForeignKey("website_id")]
        public virtual WebSite? WebSite { get; set; }

        public string? websitepassword_username { get; set; }
        public string? websitepassword_password { get; set; }
        public string? websitepassword_description { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime? updated_at { get; set; }



    }
}
