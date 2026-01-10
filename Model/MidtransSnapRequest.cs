namespace Kantin_Paramadina.Model
{
    public class MidtransSnapRequest
    {
        public TransactionDetails transaction_details { get; set; }
        public CustomerDetails customer_details { get; set; }
    }

    public class TransactionDetails
    {
        public string order_id { get; set; }
        public int gross_amount { get; set; }
    }

    public class CustomerDetails
    {
        public string first_name { get; set; }
        public string email { get; set; }
    }
}
