namespace Kantin_Paramadina.DTO
{
    public class TransactionFormDto
    {
        public string TransactionJson { get; set; } = null!;
        public IFormFile? PaymentProof { get; set; }
    }
}
