using Kantin_Paramadina.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kantin_Paramadina.Services
{
    public class MidtransSnapService
    {
        private readonly HttpClient _httpClient;

        public MidtransSnapService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<MidtransSnapResponse?> CreateQrisTransactionAsync(string orderId, decimal amount)
        {
            var payload = new
            {
                transaction_details = new
                {
                    order_id = orderId,
                    gross_amount = amount
                },
                payment_type = "qris",
                qris = new { }
            };

            // Ganti dengan server key sandbox
            var serverKey = "SB-Mid-server-YourDummyKey";
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{serverKey}:"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sandbox.midtrans.com/v2/charge")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<MidtransSnapResponse>();
        }
    }
}
