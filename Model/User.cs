using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.Model
{
    public class User
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(100)] public string Username { get; set; } = null!;
        [Required] public string PasswordHash { get; set; } = null!;
        [Required, MaxLength(20)] public string Role { get; set; } = "Customer";
        [MaxLength(100)] public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? OutletId { get; set; }
        [ForeignKey("OutletId")]
        public Outlet? Outlet { get; set; }

        public ICollection<UserToken>? Tokens { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
