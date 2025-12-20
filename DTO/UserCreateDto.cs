using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.DTO
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "Customer";

        [StringLength(100)]
        public string? FullName { get; set; }

        public int? OutletId { get; set; }
    }
}
