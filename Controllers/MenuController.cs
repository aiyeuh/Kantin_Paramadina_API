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
    public async Task<ActionResult<MenuItemDto>> Create([FromBody] MenuItemCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<MenuItem>(dto);
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
    public async Task<IActionResult> Update(int id, [FromBody] MenuItemUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = await _db.MenuItems
            .Include(m => m.Stock)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (entity == null)
            return NotFound();

        _mapper.Map(dto, entity);

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