using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.Model
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<TransactionItem>? Items { get; set; }
        public int? OutletId { get; set; }
        public Outlet? Outlet { get; set; }
        public int UserId { get; set; }  // <-- User pemilik transaksi
        public User User { get; set; } = null!;

        public string PaymentMethod { get; set; } = "COD"; // "COD" atau "QRIS"
        public string? PaymentProofPath { get; set; } // path bukti pembayaran QRIS
    }
}
