using System.ComponentModel.DataAnnotations;

namespace BugraLife.Models
{
    public class Daily
    {
        [Key]
        public int daily_id { get; set; }

        public string? daily_description { get; set; } // string? (nullable) olması daha güvenli olabilir

        public DateTime daily_date { get; set; }

        public DailyStatus daily_status { get; set; }
    }

    public enum DailyStatus
    {
        [Display(Name = "Kötü")]
        Kotu = 1,

        [Display(Name = "Orta")]
        Orta = 2,

        [Display(Name = "İyi")]
        Iyi = 3,

        [Display(Name = "Süper")]
        Super = 4
    }
}
