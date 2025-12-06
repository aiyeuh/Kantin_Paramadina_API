namespace Kantin_Paramadina.DTO
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string? OutletName { get; set; }
        public List<TransactionItemDto>? Items { get; set; }

    }
}
