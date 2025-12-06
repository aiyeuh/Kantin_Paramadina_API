using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.DTO
{
    public class TransactionItemCreateDto
    {
        [Required]
        public int MenuItemId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }
    }
}
