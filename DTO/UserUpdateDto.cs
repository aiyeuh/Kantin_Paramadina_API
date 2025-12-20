using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.DTO
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = null!;

        [StringLength(100)]
        public string? Password { get; set; }

        [Required]
        public string Role { get; set; } = "Customer";

        [StringLength(100)]
        public string? FullName { get; set; }

        public int? OutletId { get; set; }
    }
}
