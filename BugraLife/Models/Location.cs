using System.ComponentModel.DataAnnotations;

namespace BugraLife.Models
{
    public class Location
    {
        [Key]
        public int location_id { get; set; }
        public string location_name { get; set; }

        public string location_address { get; set; }

        public string location_link { get; set; }
    }
}
