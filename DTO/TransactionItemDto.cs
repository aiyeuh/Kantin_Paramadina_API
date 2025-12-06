namespace Kantin_Paramadina.DTO
{
    public class TransactionItemDto
    {
        public int MenuItemId { get; set; }
        public string MenuName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;
    }
}
