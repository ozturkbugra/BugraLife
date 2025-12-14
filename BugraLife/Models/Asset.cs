using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugraLife.Models
{
    public class Asset
    {
        [Key]
        public int asset_id { get; set; }
        public int ingredient_id { get; set; }
        [ForeignKey("ingredient_id")]
        public virtual Ingredient? Ingredient { get; set; }

        public int person_id { get; set; }
        [ForeignKey("person_id")]
        public virtual Person? Person { get; set; }

        public string asset_description { get; set; }
        public DateTime asset_date { get; set; }
        public decimal asset_amount { get; set; }
    }
}
