using BookingAdmin.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Manager,BookingStaff,Viewer")]
public class SaleEntriesController : Controller
{
    private const int PageSize = 50;
    private readonly AppDbContext _db;

    public SaleEntriesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(
        string? sale, string? source, string? status, int? month, int? year, string? q, int page = 1)
    {
        var query = _db.SaleEntries.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(sale)) query = query.Where(s => s.Sale == sale);
        if (!string.IsNullOrWhiteSpace(source)) query = query.Where(s => s.Source == source);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(s => s.Status == status);
        if (year.HasValue) query = query.Where(s => s.TrackingDate!.Value.Year == year);
        if (month.HasValue) query = query.Where(s => s.TrackingDate!.Value.Month == month);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q.Trim()}%";
            query = query.Where(s =>
                EF.Functions.Like(s.BookingCode ?? string.Empty, like) ||
                EF.Functions.Like(s.Agency ?? string.Empty, like));
        }

        var total = await query.CountAsync();
        var totalRevenue = await query.SumAsync(s => s.Price ?? 0m);
        var items = await query
            .OrderByDescending(s => s.TrackingDate)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .ToListAsync();

        ViewBag.Sales = await _db.SaleEntries.Select(s => s.Sale!).Where(s => s != null).Distinct().OrderBy(s => s).ToListAsync();
        ViewBag.Sources = await _db.SaleEntries.Select(s => s.Source!).Where(s => s != null).Distinct().OrderBy(s => s).ToListAsync();
        ViewBag.Statuses = await _db.SaleEntries.Select(s => s.Status!).Where(s => s != null).Distinct().OrderBy(s => s).ToListAsync();
        ViewBag.Total = total;
        ViewBag.TotalRevenue = totalRevenue;
        ViewBag.Page = page;
        ViewBag.PageSize = PageSize;
        ViewBag.Filter = new { sale, source, status, month, year, q };

        return View(items);
    }
}
