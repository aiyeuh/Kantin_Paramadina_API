using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.DTO
{
    public class MenuItemCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }

        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Required]
        public int OutletId { get; set; }

        [Range(0, 1000)]
        public int? InitialStockQuantity { get; set; }

        // Support both file upload dan URL string
        public IFormFile? ImageFile { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }
    }
}
