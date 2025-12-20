using AutoMapper;
using Kantin_Paramadina.DTO;
using Kantin_Paramadina.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kantin_Paramadina.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OutletsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public OutletsController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OutletDto>>> GetAll()
    {
        var outlets = await _db.Outlets
            .Include(o => o.MenuItems!)
                .ThenInclude(m => m.Stock)
            .ToListAsync();

        var result = _mapper.Map<IEnumerable<OutletDto>>(outlets);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OutletDto>> GetById(int id)
    {
        var outlet = await _db.Outlets
            .Include(o => o.MenuItems!)
                .ThenInclude(m => m.Stock)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (outlet == null)
            return NotFound();

        return Ok(_mapper.Map<OutletDto>(outlet));
    }

    [HttpPost]
    public async Task<ActionResult<OutletDto>> Create([FromBody] OutletCreateDto dto)
    {
        // Ambil role dari JWT
        var role = User.FindFirst("role")?.Value;
        if (role == null || role.ToLower() != "admin")
            return Unauthorized(new { message = "Hanya Admin yang dapat membuat outlet." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var outlet = _mapper.Map<Outlet>(dto);
        _db.Outlets.Add(outlet);
        await _db.SaveChangesAsync();

        var result = await _db.Outlets
            .Include(o => o.MenuItems!)
                .ThenInclude(m => m.Stock)
            .FirstOrDefaultAsync(o => o.Id == outlet.Id);

        return CreatedAtAction(nameof(GetById), new { id = outlet.Id }, _mapper.Map<OutletDto>(result));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateOutlet(int id, [FromBody] OutletUpdateDto dto)
    {
        // Ambil role & outletId dari JWT
        var role = User.FindFirst("role")?.Value;
        var userOutletIdClaim = User.FindFirst("OutletId")?.Value;

        int userOutletId = 0;
        if (!string.IsNullOrEmpty(userOutletIdClaim))
            int.TryParse(userOutletIdClaim, out userOutletId);

        // === RULE AKSES ===
        if (role?.ToLower() != "admin")
        {
            if (userOutletId == 0)
                return Unauthorized(new { message = "Akses ditolak. Akun Anda tidak terdaftar pada outlet manapun." });

            if (userOutletId != id)
                return Unauthorized(new { message = "Anda tidak memiliki akses ke outlet ini."});
        }

        // === Ambil data outlet ===
        var outlet = await _db.Outlets.FirstOrDefaultAsync(o => o.Id == id);
        if (outlet == null)
            return NotFound("Outlet tidak ditemukan.");

        // === Update field yang boleh ===
        outlet.Name = dto.Name;
        outlet.QrisImageUrl = dto.QrisImageUrl;

        // === Simpan ===
        await _db.SaveChangesAsync();

        return Ok("Outlet berhasil diperbarui.");
    }
}
