namespace Kantin_Paramadina.DTO
{
    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public int OutletId { get; set; }
        public string? OutletName { get; set; }
        public int? StockQuantity { get; set; }
    }
}
