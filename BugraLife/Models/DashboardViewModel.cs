namespace BugraLife.Models
{
    public class DashboardViewModel
    {
        // Diğer dashboard verileri (Toplam bakiye vs.) buraya eklenebilir.
        // Biz şimdilik sabit gider listesine odaklanıyoruz.
        public List<FixedExpenseStatus> FixedExpenseStatuses { get; set; }
    }

    public class FixedExpenseStatus
    {
        public string ExpenseName { get; set; } // Örn: Kira
        public int PaymentDay { get; set; }     // Örn: 1
        public decimal EstimatedAmount { get; set; } // Tutar varsa
        public bool IsPaid { get; set; }        // Ödendi mi?
        public int DaysDiff { get; set; }       // Pozitif: Kaldı, Negatif: Geçti
        public DateTime DueDate { get; set; }   // Son Ödeme Tarihi
    }
}