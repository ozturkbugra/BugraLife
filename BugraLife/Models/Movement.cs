using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugraLife.Models
{
    public class Movement
    {
        [Key]
        public int movement_id { get; set; }
        public int debtor_id { get; set; }
        [ForeignKey("debtor_id")]
        public virtual Debtor? Debtor { get; set; }

        public int ingredient_id { get; set; }
        [ForeignKey("ingredient_id")]
        public virtual Ingredient? Ingredient { get; set; }

        public int person_id { get; set; }
        [ForeignKey("person_id")]
        public virtual Person? Person { get; set; }
        
        public decimal movement_amount { get; set; }
        public DateTime movement_date { get; set; }

        public string movement_description { get; set; }
    }
}
