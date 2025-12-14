using System.ComponentModel.DataAnnotations;

namespace BugraLife.Models
{
    public class Ingredient
    {
        [Key]
        public int ingredient_id { get; set; }
        public string ingredient_name { get; set; }
    }
}
