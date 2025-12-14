using System.ComponentModel.DataAnnotations;

namespace BugraLife.Models
{
    public class UnPlannedToDo
    {
        [Key]
        public int unplannedtodo_id { get; set; }
        public string unplannedtodo_description { get; set; }
        public DateTime unplannedtodo_createdat { get; set; }
        public bool unplannedtodo_done { get; set; }
    }
}
