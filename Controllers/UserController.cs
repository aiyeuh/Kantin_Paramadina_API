using AutoMapper;
using Kantin_Paramadina.DTO;
using Kantin_Paramadina.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kantin_Paramadina.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public UserController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // GET: api/user
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _db.Users.ToListAsync();
        return Ok(_mapper.Map<IEnumerable<UserDto>>(users));
    }

    // GET: api/user/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(_mapper.Map<UserDto>(user));
    }

    // POST: api/user
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserCreateDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == null || role.ToLower() != "admin")
            return Unauthorized(new { message = "Hanya admin yang dapat membuat user." });

        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest(new { message = "Username sudah digunakan." });

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            FullName = dto.FullName,
            OutletId = dto.Role == "Cashier" ? dto.OutletId : null
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, _mapper.Map<UserDto>(user));
    }

    // PUT: api/user/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var callerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int callerId = 0;
        if (!string.IsNullOrEmpty(callerIdClaim)) int.TryParse(callerIdClaim, out callerId);

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        // only admin or the user themself can update
        if (role == null) return Unauthorized();
        if (role.ToLower() != "admin" && callerId != id)
            return Unauthorized();

        user.Username = dto.Username;
        if (!string.IsNullOrEmpty(dto.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.Role = dto.Role;
        user.FullName = dto.FullName;
        user.OutletId = dto.Role == "Cashier" ? dto.OutletId : null;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/user/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == null || role.ToLower() != "admin")
            return Unauthorized();

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
