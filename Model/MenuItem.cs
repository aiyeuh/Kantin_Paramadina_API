using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.Model
{
    public class MenuItem
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
        public int OutletId { get; set; }
        public Outlet? Outlet { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        // stok referensi
        public Stock? Stock { get; set; }
    }
}
