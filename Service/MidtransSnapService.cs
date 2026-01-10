using Kantin_Paramadina.Model;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class MidtransSnapService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public MidtransSnapService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<MidtransSnapResponse?> CreateSnapTokenAsync(MidtransSnapRequest requestData)
    {
        var serverKey = _config["Midtrans:ServerKey"];

        var authToken = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{serverKey}:")
        );

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://app.sandbox.midtrans.com/snap/v1/transactions"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", authToken);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestData),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Midtrans error: {error}");
        }

        return await response.Content
            .ReadFromJsonAsync<MidtransSnapResponse>();
    }
}
