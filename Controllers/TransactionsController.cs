using AutoMapper;
using Kantin_Paramadina.DTO;
using Kantin_Paramadina.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Kantin_Paramadina.Hubs;

namespace Kantin_Paramadina.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IHubContext<TransactionHub> _hub;
    private readonly MidtransSnapService _midtransSnapService;

    public TransactionsController(ApplicationDbContext db, IMapper mapper, IHubContext<TransactionHub> hub)
    {
        _db = db;
        _mapper = mapper;
        _hub = hub;
    }

    [HttpPost]
    [RequestSizeLimit(10_000_000)] // Maks 10 MB
    public async Task<ActionResult<object>> Create([FromForm] TransactionFormDto form)
    {
        // Ambil UserId dari token
        var userIdClaim = User.FindFirst("userId")?.Value;
        var roleClaim = User.FindFirst("role")?.Value;

        if (userIdClaim == null || roleClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        //Deserialize JSON string ke DTO transaksi
        TransactionCreateDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<TransactionCreateDto>(
                form.TransactionJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Format JSON transaksi tidak valid." });
        }

        if (dto == null)
            return BadRequest(new { message = "Data transaksi kosong atau rusak." });

        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(new { message = "Harus ada minimal 1 item." });

        if (dto.PaymentMethod == "QRIS" && form.PaymentProof == null)
            return BadRequest(new { message = "Bukti pembayaran QRIS wajib diunggah." });

        var strategy = _db.Database.CreateExecutionStrategy();

        try
        {
            Transaction? newTransaction = null;

            await strategy.ExecuteAsync(async () =>
            {
                await using var dbTransaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    newTransaction = _mapper.Map<Transaction>(dto);
                    newTransaction.TotalAmount = 0;
                    newTransaction.UserId = userId;

                    // 🔹 Validasi outlet
                    var outlet = await _db.Outlets.FindAsync(dto.OutletId);
                    if (outlet == null)
                        throw new Exception($"Outlet dengan ID {dto.OutletId} tidak ditemukan.");

                    // 🔹 Proses setiap item transaksi
                    foreach (var itemDto in dto.Items)
                    {
                        var menu = await _db.MenuItems
                            .Include(m => m.Stock)
                            .FirstOrDefaultAsync(m => m.Id == itemDto.MenuItemId);

                        if (menu == null)
                            throw new Exception($"Menu dengan ID {itemDto.MenuItemId} tidak ditemukan.");
                        if (menu.Stock == null)
                            throw new Exception($"Stok untuk menu '{menu.Name}' belum dibuat.");
                        if (menu.Stock.Quantity < itemDto.Quantity)
                            throw new Exception($"Stok '{menu.Name}' tidak mencukupi. Sisa: {menu.Stock.Quantity}");

                        // Kurangi stok
                        menu.Stock.Quantity -= itemDto.Quantity;

                        // Tambahkan ke daftar item transaksi
                        var item = new TransactionItem
                        {
                            MenuItemId = itemDto.MenuItemId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = menu.Price
                        };

                        newTransaction.Items ??= new List<TransactionItem>();
                        newTransaction.Items.Add(item);

                        // Hitung total
                        newTransaction.TotalAmount += menu.Price * itemDto.Quantity;
                    }
                    // 🔹 Simpan bukti QRIS (jika ada)
                    if (dto.PaymentMethod == "QRIS" && form.PaymentProof != null)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var ext = Path.GetExtension(form.PaymentProof.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(ext))
                            throw new Exception("Format file tidak valid. Hanya .jpg/.jpeg/.png diperbolehkan.");

                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "qris");
                        if (!Directory.Exists(uploadDir))
                            Directory.CreateDirectory(uploadDir);

                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(uploadDir, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                            await form.PaymentProof.CopyToAsync(stream);

                        newTransaction.PaymentProofPath = $"/uploads/qris/{fileName}";
                    }

                    // 🔹 Simpan transaksi ke DB
                    _db.Transactions.Add(newTransaction);
                    await _db.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                    
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            });

            // Send realtime notification to outlet group
            try
            {
                if (newTransaction?.OutletId != null)
                {
                    var payload = new
                    {
                        id = newTransaction.Id,
                        customerName = newTransaction.CustomerName,
                        totalAmount = newTransaction.TotalAmount,
                        createdAt = newTransaction.CreatedAt
                    };

                    await _hub.Clients.Group($"Outlet_{newTransaction.OutletId}")
                        .SendAsync("TransactionCreated", payload);
                }
            }
            catch
            {
                // ignore hub errors
            }

            return Ok(new
            {
                message = $"Transaksi atas nama {newTransaction!.CustomerName} berhasil dibuat.",
                transactionId = newTransaction.Id,
                totalAmount = newTransaction.TotalAmount,
                paymentMethod = newTransaction.PaymentMethod,
                paymentProofPath = newTransaction.PaymentProofPath
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Gagal memproses transaksi",
                error = ex.Message
            });
        }
    }
    //GET: api/transactions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAll()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        var roleClaim = User.FindFirst("role")?.Value;

        return Ok(new 
        {
            DebugUser = User.Identity.Name,
            DebugUserId = userIdClaim, 
            DebugRole = roleClaim,
            Message = "Ini adalah mode debug"
        });

        if (userIdClaim == null || roleClaim == null)
            return Unauthorized(new { message = "Invalid token claims" });

        var userId = int.Parse(userIdClaim);
        var role = roleClaim;

        IQueryable<Transaction> query = _db.Transactions
            .Include(t => t.Outlet)
            .Include(t => t.Items)
                .ThenInclude(i => i.MenuItem)
            .OrderByDescending(t => t.CreatedAt);

        if (role == "Admin")
        {
            // Admin bisa lihat semua transaksi
        }
        else if (role == "Cashier")
        {
            // Hanya transaksi outlet cashier
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();
            if (!user.OutletId.HasValue) return Unauthorized();

            query = query.Where(t => t.OutletId == user.OutletId.Value);
        }
        else
        {
            // Customer biasa
            query = query.Where(t => t.UserId == userId);
        }

        var transactions = await query.ToListAsync();
        return Ok(_mapper.Map<IEnumerable<TransactionDto>>(transactions));
    }

    //GET: api/transactions/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetById(int id)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        var roleClaim = User.FindFirst("role")?.Value;

        if (userIdClaim == null || roleClaim == null)
            return Unauthorized(new { message = "Invalid token claims" });

        var userId = int.Parse(userIdClaim);
        var role = roleClaim;

        IQueryable<Transaction> query = _db.Transactions
            .Include(t => t.Outlet)
            .Include(t => t.Items)
                .ThenInclude(i => i.MenuItem)
            .Where(t => t.Id == id);

        if (role == "Admin") { }
        else if (role == "Cashier")
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || !user.OutletId.HasValue)
                return Unauthorized();
            query = query.Where(t => t.OutletId == user.OutletId.Value);
        }
        else
        {
            query = query.Where(t => t.UserId == userId);
        }

        var transaction = await query.FirstOrDefaultAsync();
        if (transaction == null) return NotFound();

        return Ok(_mapper.Map<TransactionDto>(transaction));
    }

    //GET: api/transactions/recent?count=5
    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetRecent([FromQuery] int count = 5)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        var roleClaim = User.FindFirst("role")?.Value;
        

        if (userIdClaim == null || roleClaim == null)
            return Unauthorized(new { message = "Invalid token claims" });

        var userId = int.Parse(userIdClaim);
        var role = roleClaim;

        IQueryable<Transaction> query = _db.Transactions
            .Include(t => t.Outlet)
            .Include(t => t.Items)
                .ThenInclude(i => i.MenuItem)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count);

        if (role == "Admin") { }
        else if (role == "Cashier")
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || !user.OutletId.HasValue)
                return Unauthorized();
            query = query.Where(t => t.OutletId == user.OutletId.Value);
        }
        else
        {
            query = query.Where(t => t.UserId == userId);
        }

        var transactions = await query.ToListAsync();
        return Ok(_mapper.Map<IEnumerable<TransactionDto>>(transactions));
    }
    // DELETE: api/transactions/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        var roleClaim = User.FindFirst("role")?.Value;

        if (userIdClaim == null || roleClaim == null)
            return Unauthorized(new { message = "Invalid token claims" });

        var userId = int.Parse(userIdClaim);
        var role = roleClaim;

        IQueryable<Transaction> query = _db.Transactions
            .Include(t => t.Items!)
                .ThenInclude(i => i.MenuItem)
                .ThenInclude(m => m.Stock)
            .Where(t => t.Id == id);

        if (role == "Admin") { }
        else if (role == "Cashier")
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || !user.OutletId.HasValue)
                return Unauthorized();
            query = query.Where(t => t.OutletId == user.OutletId.Value);
        }
        else
        {
            query = query.Where(t => t.UserId == userId);
        }

        var transaction = await query.FirstOrDefaultAsync();
        if (transaction == null)
            return NotFound(new { message = "Transaksi tidak ditemukan atau tidak punya akses." });

        foreach (var item in transaction.Items!)
        {
            if (item.MenuItem?.Stock != null)
                item.MenuItem.Stock.Quantity += item.Quantity;
        }

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Transaksi {transaction.Id} dibatalkan dan stok dikembalikan." });
    }
}
