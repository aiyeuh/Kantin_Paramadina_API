using AutoMapper;
using Kantin_Paramadina.DTO;
using Kantin_Paramadina.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kantin_Paramadina.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public MenuController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // ✅ GET: api/menu
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetAll()
    {
        var menuItems = await _db.MenuItems
            .Include(m => m.Stock)
            .Include(m => m.Outlet)
            .ToListAsync();

        var result = _mapper.Map<IEnumerable<MenuItemDto>>(menuItems);
        return Ok(result);
    }

    // ✅ GET: api/menu/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MenuItemDto>> GetById(int id)
    {
        var item = await _db.MenuItems
            .Include(m => m.Stock)
            .Include(m => m.Outlet)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null)
            return NotFound();

        return Ok(_mapper.Map<MenuItemDto>(item));
    }

    // ✅ POST: api/menu
    [HttpPost]
    [RequestSizeLimit(10_000_000)] // Maks 10 MB untuk file
    public async Task<ActionResult<MenuItemDto>> Create([FromForm] MenuItemCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<MenuItem>(dto);

        // 🔹 Proses upload gambar jika ada
        if (dto.ImageFile != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(dto.ImageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Format file tidak valid. Hanya .jpg/.jpeg/.png/.gif diperbolehkan." });

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "menu");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await dto.ImageFile.CopyToAsync(stream);

            entity.ImageUrl = $"/uploads/menu/{fileName}";
        }

        _db.MenuItems.Add(entity);
        await _db.SaveChangesAsync();

        // Tambahkan stok awal jika diberikan
        if (dto.InitialStockQuantity.HasValue)
        {
            _db.Stocks.Add(new Stock
            {
                MenuItemId = entity.Id,
                Quantity = dto.InitialStockQuantity.Value
            });
            await _db.SaveChangesAsync();
        }

        var result = await _db.MenuItems
            .Include(m => m.Stock)
            .Include(m => m.Outlet)
            .FirstOrDefaultAsync(m => m.Id == entity.Id);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, _mapper.Map<MenuItemDto>(result));
    }

    // ✅ PUT: api/menu/5
    [HttpPut("{id}")]
    [RequestSizeLimit(10_000_000)] // Maks 10 MB untuk file
    public async Task<IActionResult> Update(int id, [FromForm] MenuItemUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = await _db.MenuItems
            .Include(m => m.Stock)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null)
            return NotFound();

        // Jika ada file upload gambar baru
        if (dto.ImageFile != null)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(dto.ImageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                return BadRequest(new { message = "Format file tidak valid. Hanya .jpg/.jpeg/.png/.gif diperbolehkan." });

            // Hapus file lama jika ada
            if (!string.IsNullOrEmpty(entity.ImageUrl) && entity.ImageUrl.StartsWith("/uploads/menu/"))
            {
                var oldFileName = Path.GetFileName(entity.ImageUrl);
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "menu", oldFileName);
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "menu");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await dto.ImageFile.CopyToAsync(stream);

            entity.ImageUrl = $"/uploads/menu/{fileName}";
        }
        else if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            // Gunakan ImageUrl string jika tidak ada file upload
            entity.ImageUrl = dto.ImageUrl;
        }

        // Update property lainnya
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Price = dto.Price;

        if (entity.Stock != null && dto.StockQuantity.HasValue)
            entity.Stock.Quantity = dto.StockQuantity.Value;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ✅ DELETE: api/menu/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.MenuItems.FindAsync(id);
        if (entity == null)
            return NotFound();

        // Hapus stok terlebih dahulu jika ada
        var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.MenuItemId == id);
        if (stock != null)
            _db.Stocks.Remove(stock);

        _db.MenuItems.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}