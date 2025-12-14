using System.ComponentModel.DataAnnotations;

namespace BugraLife.Models
{
    public class PlannedToDo
    {
        [Key]
        public int plannedtodo_id { get; set; }
        public string plannedtodo_description { get; set; }
        public DateTime plannedtodo_date { get; set; }
        public bool plannedtodo_done { get; set; }
    }
}
