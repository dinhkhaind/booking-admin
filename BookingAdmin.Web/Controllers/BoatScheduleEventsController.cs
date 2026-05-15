using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class BoatScheduleEventsController : BaseController
{
    public BoatScheduleEventsController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        var events = await _db.BoatScheduleEvents
            .Include(e => e.Boat)
            .Include(e => e.CreatedByUser)
            .OrderByDescending(e => e.FromDate)
            .ToListAsync();

        ViewBag.Boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();

        return View(events);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoatScheduleEventRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Invalid data" });

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdString, out int userId);

        if (request.ToDate < request.FromDate)
            return Json(new { success = false, error = "Ngày kết thúc phải sau ngày bắt đầu" });

        var boatEvent = new BoatScheduleEvent
        {
            BoatId = request.BoatId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Type = request.Type,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _db.BoatScheduleEvents.Add(boatEvent);
        await _db.SaveChangesAsync();

        return Json(new { success = true, message = "Thêm sự kiện thành công" });
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] UpdateBoatScheduleEventRequest request)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Invalid data" });

        var boatEvent = await _db.BoatScheduleEvents.FindAsync(request.Id);
        if (boatEvent == null)
            return Json(new { success = false, error = "Sự kiện không tồn tại" });

        if (request.ToDate < request.FromDate)
            return Json(new { success = false, error = "Ngày kết thúc phải sau ngày bắt đầu" });

        boatEvent.BoatId = request.BoatId;
        boatEvent.FromDate = request.FromDate;
        boatEvent.ToDate = request.ToDate;
        boatEvent.Type = request.Type;
        boatEvent.Reason = request.Reason;

        _db.BoatScheduleEvents.Update(boatEvent);
        await _db.SaveChangesAsync();

        return Json(new { success = true, message = "Cập nhật sự kiện thành công" });
    }

    [HttpGet]
    public async Task<IActionResult> GetEvent(int id)
    {
        var boatEvent = await _db.BoatScheduleEvents
            .Include(e => e.Boat)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (boatEvent == null)
            return Json(new { success = false, error = "Sự kiện không tồn tại" });

        return Json(new
        {
            success = true,
            data = new
            {
                boatEvent.Id,
                boatEvent.BoatId,
                boatEvent.FromDate,
                boatEvent.ToDate,
                boatEvent.Type,
                boatEvent.Reason,
                boatEvent.Boat?.Name
            }
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var boatEvent = await _db.BoatScheduleEvents.FindAsync(id);
        if (boatEvent == null)
            return Json(new { success = false, error = "Sự kiện không tồn tại" });

        _db.BoatScheduleEvents.Remove(boatEvent);
        await _db.SaveChangesAsync();

        return Json(new { success = true, message = "Xoá sự kiện thành công" });
    }

    [HttpGet("api/boat-events")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBoatEvents(int boatId, int month, int year)
    {
        var monthStart = new DateOnly(year, month, 1);
        var monthEnd = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        var events = await _db.BoatScheduleEvents
            .Where(e => e.BoatId == boatId &&
                       e.FromDate <= monthEnd &&
                       e.ToDate >= monthStart)
            .Select(e => new
            {
                e.Id,
                e.Type,
                e.Reason,
                e.FromDate,
                e.ToDate,
                FromDateStr = e.FromDate.ToString("yyyy-MM-dd"),
                ToDateStr = e.ToDate.ToString("yyyy-MM-dd")
            })
            .ToListAsync();

        return Json(events);
    }
}

public class CreateBoatScheduleEventRequest
{
    public int BoatId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class UpdateBoatScheduleEventRequest
{
    public int Id { get; set; }
    public int BoatId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
