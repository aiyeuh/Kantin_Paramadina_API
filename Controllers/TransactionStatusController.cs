using Kantin_Paramadina.Model;
using Kantin_Paramadina.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kantin_Paramadina.Controllers;

[ApiController]
[Route("api/transactions/status")]
[Authorize]
public class TransactionStatusController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<TransactionHub> _hub;

    public TransactionStatusController(ApplicationDbContext db, IHubContext<TransactionHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    // PUT api/transactions/status/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] int status)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(role))
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var query = _db.Transactions
            .Include(t => t.Items!)
                .ThenInclude(i => i.MenuItem)
                    .ThenInclude(m => m.Stock)
            .Include(t => t.Outlet)
            .Where(t => t.Id == id);

        if (role == "Admin") { }
        else if (role == "Cashier")
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || !user.OutletId.HasValue) return Unauthorized();
            query = query.Where(t => t.OutletId == user.OutletId.Value);
        }
        else
        {
            query = query.Where(t => t.UserId == userId);
        }

        var transaction = await query.FirstOrDefaultAsync();
        if (transaction == null) return NotFound(new { message = "Transaksi tidak ditemukan atau tidak punya akses." });

        if (status < 1 || status > 5) return BadRequest(new { message = "Status tidak valid." });

        // bila dibatalkan, kembalikan stok jika belum dibatalkan
        if (status == 5 && transaction.Status != 5)
        {
            foreach (var item in transaction.Items!)
            {
                if (item.MenuItem?.Stock != null)
                    item.MenuItem.Stock.Quantity += item.Quantity;
            }
        }

        transaction.Status = status;
        await _db.SaveChangesAsync();

        // Kirim notifikasi realtime ke grup outlet
        try
        {
            if (transaction.OutletId != null)
            {
                var payload = new
                {
                    id = transaction.Id,
                    status = transaction.Status,
                    updatedAt = DateTime.UtcNow
                };
                await _hub.Clients.Group($"Outlet_{transaction.OutletId}")
                    .SendAsync("TransactionStatusUpdated", payload);
            }
        }
        catch
        {
            // swallow hub errors
        }

        return Ok(new { message = "Status transaksi diperbarui.", status = transaction.Status });
    }
}
