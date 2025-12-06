using AutoMapper;
using Kantin_Paramadina.DTO;
using Kantin_Paramadina.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kantin_Paramadina.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public ReportsController(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    //GET: api/reports/sales/daily?date=2025-11-08
    [HttpGet("sales/daily")]
    public async Task<ActionResult<object>> SalesDaily(DateTime? date)
    {
        var day = (date ?? DateTime.UtcNow).Date;
        var next = day.AddDays(1);

        var transactions = await _db.Transactions
            .Include(t => t.Outlet)
            .Where(t => t.CreatedAt >= day && t.CreatedAt < next)
            .ToListAsync();

        var totalSales = transactions.Sum(t => t.TotalAmount);
        var outletGroups = transactions
            .GroupBy(t => t.Outlet!.Name)
            .Select(g => new
            {
                Outlet = g.Key,
                Transactions = g.Count(),
                TotalSales = g.Sum(x => x.TotalAmount)
            })
            .ToList();

        return Ok(new
        {
            Date = day,
            TotalOutlets = outletGroups.Count,
            TotalSales = totalSales,
            Details = outletGroups
        });
    }

    //GET: api/reports/stock/byoutlet/1
    [HttpGet("stock/byoutlet/{outletId}")]
    public async Task<ActionResult<IEnumerable<MenuItemDto>>> StockByOutlet(int outletId)
    {
        var items = await _db.MenuItems
            .Include(m => m.Stock)
            .Where(m => m.OutletId == outletId)
            .ToListAsync();

        var result = _mapper.Map<IEnumerable<MenuItemDto>>(items);
        return Ok(result);
    }
}