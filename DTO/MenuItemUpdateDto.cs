using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.DTO
{
    public class MenuItemUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Range(0, 1000)]
        public int? StockQuantity { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }
    }
}
