using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.Model
{
    public class Outlet
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = null!;
        public string? Location { get; set; }
        public string? QrisImageUrl { get; set; }
        public ICollection<MenuItem>? MenuItems { get; set; }
    }
}
