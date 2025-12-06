using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Kantin_Paramadina.Model
{
    public class TransactionItem
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }

        public decimal SubTotal => UnitPrice * Quantity;
    }
}
