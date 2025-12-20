using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.DTO
{
    public class OutletCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(255)]
        public string? QrisImageUrl { get; set; }
    }
}
