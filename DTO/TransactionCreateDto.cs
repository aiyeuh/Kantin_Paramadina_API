using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.DTO
{
    public class TransactionCreateDto
    {
        [Required]
        public int OutletId { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = null!;

        [Required]
        [MinLength(1, ErrorMessage = "Harus ada minimal 1 item.")]
        public List<TransactionItemCreateDto> Items { get; set; } = new();

        [Required]
        [RegularExpression("^(COD|QRIS)$", ErrorMessage = "Metode pembayaran harus COD atau QRIS.")]
        public string PaymentMethod { get; set; } = "COD";
    }
}
