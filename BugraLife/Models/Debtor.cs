using System.ComponentModel.DataAnnotations;

namespace BugraLife.Models
{
    public class Debtor
    {
        [Key]
        public int debtor_id { get; set; }
        public string debtor_name { get; set; }
        
    }
}
