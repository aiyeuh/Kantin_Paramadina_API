using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.Model
{
    public class UserToken
    {
        [Key] public int Id { get; set; }

        [Required] public int UserId { get; set; }
        [Required] public string Token { get; set; } = null!;
        public DateTime ExpiredAt { get; set; }
        public bool Revoked { get; set; } = false;

        [ForeignKey("UserId")] public User? User { get; set; }
    }
}
