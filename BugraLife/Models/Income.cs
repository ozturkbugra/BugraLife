using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugraLife.Models
{
    public class Income
    {
        [Key]
        public int income_id { get; set; }

        public int incometype_id { get; set; }

        [ForeignKey("incometype_id")]
        public virtual IncomeType? IncomeType { get; set; }


        public int paymenttype_id { get; set; }

        [ForeignKey("paymenttype_id")]
        public virtual PaymentType? PaymentType { get; set; }


        public int person_id { get; set; }

        [ForeignKey("person_id")]
        public virtual Person? Person { get; set; }

        public decimal income_amount { get; set; }
        public DateTime income_date { get; set; }
        public string income_description { get; set; }
        public bool is_bankmovement { get; set; }
    }
}
