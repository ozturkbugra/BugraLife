using System.ComponentModel.DataAnnotations;

namespace BugraLife.Models
{
    public class IncomeType
    {
        [Key]
        public int incometype_id { get; set; }
        public string incometype_name { get; set; }
        public int incometype_order { get; set; }
        public bool is_bank { get; set; }
    }
}
