using Kantin_Paramadina.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly MidtransSnapService _midtrans;

    public PaymentController(MidtransSnapService midtrans)
    {
        _midtrans = midtrans;
    }

    [Authorize]
    [HttpPost("snap")]
    public async Task<IActionResult> CreateSnap()
    {
        // 🔹 Ambil data user dari JWT\
        var fullName = User.FindFirst("username")?.Value;
        var userIdClaim = User.FindFirst("userId")?.Value;
        var roleClaim = User.FindFirst("role")?.Value;

        if (userIdClaim == null || roleClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        // 🔹 Pisahkan nama depan (Midtrans butuh first_name)
        var firstName = fullName?.Split(' ').FirstOrDefault() ?? "Customer";

        var request = new MidtransSnapRequest
        {
            transaction_details = new TransactionDetails
            {
                order_id = $"ORDER-{userId}-{DateTime.UtcNow.Ticks}",
                gross_amount = 75000
            },
            customer_details = new CustomerDetails
            {
                first_name = firstName,
                email = null // Bisa diisi email user jika ada
            }
        };

        var snap = await _midtrans.CreateSnapTokenAsync(request);

        return Ok(snap);
    }
}