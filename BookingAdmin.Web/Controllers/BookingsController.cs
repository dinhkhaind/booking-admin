using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Manager,BookingStaff,Viewer")]
public class BookingsController : BaseController
{
    private const int PageSize = 50;

    public BookingsController(AppDbContext db) : base(db) { }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = _db.Bookings.AsNoTracking().Include(b => b.Boat).Include(b => b.Channel).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q.Trim()}%";
            query = query.Where(b =>
                EF.Functions.Like(b.BookingCode ?? string.Empty, like) ||
                EF.Functions.Like(b.CustomerName ?? string.Empty, like) ||
                EF.Functions.Like(b.Note ?? string.Empty, like));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CheckIn).ThenBy(b => b.Boat!.Name)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .ToListAsync();

        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.PageSize = PageSize;
        ViewBag.Filter = new { q };

        return View(items);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Calendar()
    {
        var today = DateTime.Today;
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthEnd = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        var bookings = await _db.Bookings.AsNoTracking()
            .Include(b => b.Boat)
            .Include(b => b.Channel)
            .Where(b => b.CheckIn <= monthEnd && b.CheckOut >= monthStart)
            .OrderBy(b => b.Boat!.Name).ThenBy(b => b.CheckIn)
            .ToListAsync();

        ViewBag.Year = today.Year;
        ViewBag.Month = today.Month;
        return View(bookings);
    }

    [Authorize(Roles = "Admin,Manager,BookingStaff")]
    public async Task<IActionResult> Create()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdString, out int userId);
        var allowedBoatIds = await GetAllowedBoatIds(userId);

        ViewBag.Boats = await _db.Boats
            .Where(b => b.IsActive && allowedBoatIds.Contains(b.Id))
            .OrderBy(b => b.Name)
            .ToListAsync();
        ViewBag.Channels = await _db.Channels.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Employees = await _db.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
        ViewBag.Currencies = await _db.Currencies.OrderBy(c => c.Code).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager,BookingStaff")]
    public async Task<IActionResult> Create(Booking booking, List<BookingRoomQuantityViewModel>? roomQuantities)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdString, out int userId);
        var allowedBoatIds = await GetAllowedBoatIds(userId);

        if (!allowedBoatIds.Contains(booking.BoatId))
        {
            TempData["Error"] = "You don't have permission to create bookings for this boat.";
            return RedirectToAction(nameof(Create));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Boats = await _db.Boats
                .Where(b => b.IsActive && allowedBoatIds.Contains(b.Id))
                .OrderBy(b => b.Name)
                .ToListAsync();
            ViewBag.Channels = await _db.Channels.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Employees = await _db.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
            ViewBag.Currencies = await _db.Currencies.OrderBy(c => c.Code).ToListAsync();
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = string.Join(" | ", errors);
            return View(booking);
        }

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        if (roomQuantities != null && roomQuantities.Any(rq => rq.Quantity > 0))
        {
            var bookingRooms = roomQuantities
                .Where(rq => rq.Quantity > 0)
                .Select(rq => new BookingRoom
                {
                    BookingId = booking.Id,
                    RoomId = rq.RoomId,
                    Quantity = rq.Quantity
                }).ToList();
            _db.BookingRooms.AddRange(bookingRooms);
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = $"Booking '{booking.BookingCode}' created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager,BookingStaff")]
    public async Task<IActionResult> Edit(int id)
    {
        var booking = await _db.Bookings.Include(b => b.BookingRooms).FirstOrDefaultAsync(b => b.Id == id);
        if (booking == null) return NotFound();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdString, out int userId);
        var allowedBoatIds = await GetAllowedBoatIds(userId);

        if (!allowedBoatIds.Contains(booking.BoatId))
            return Forbid();

        ViewBag.Boats = await _db.Boats
            .Where(b => b.IsActive && allowedBoatIds.Contains(b.Id))
            .OrderBy(b => b.Name)
            .ToListAsync();
        ViewBag.Channels = await _db.Channels.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Employees = await _db.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
        ViewBag.Currencies = await _db.Currencies.OrderBy(c => c.Code).ToListAsync();
        ViewBag.SelectedRoomIds = booking.BookingRooms.Select(br => br.RoomId).ToList();
        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager,BookingStaff")]
    public async Task<IActionResult> Edit(int id, Booking booking, List<BookingRoomQuantityViewModel>? roomQuantities)
    {
        if (id != booking.Id) return BadRequest();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdString, out int userId);
        var allowedBoatIds = await GetAllowedBoatIds(userId);

        if (!allowedBoatIds.Contains(booking.BoatId))
        {
            TempData["Error"] = "You don't have permission to edit bookings for this boat.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Boats = await _db.Boats
                .Where(b => b.IsActive && allowedBoatIds.Contains(b.Id))
                .OrderBy(b => b.Name)
                .ToListAsync();
            ViewBag.Channels = await _db.Channels.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Employees = await _db.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
            ViewBag.Currencies = await _db.Currencies.OrderBy(c => c.Code).ToListAsync();
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = string.Join(" | ", errors);
            return View(booking);
        }

        _db.Bookings.Update(booking);
        await _db.SaveChangesAsync();

        // Update rooms
        var existingRooms = await _db.BookingRooms.Where(br => br.BookingId == id).ToListAsync();
        _db.BookingRooms.RemoveRange(existingRooms);

        if (roomQuantities != null && roomQuantities.Any(rq => rq.Quantity > 0))
        {
            var bookingRooms = roomQuantities
                .Where(rq => rq.Quantity > 0)
                .Select(rq => new BookingRoom
                {
                    BookingId = booking.Id,
                    RoomId = rq.RoomId,
                    Quantity = rq.Quantity
                }).ToList();
            _db.BookingRooms.AddRange(bookingRooms);
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Booking '{booking.BookingCode}' updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager,BookingStaff")]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        if (booking == null) return NotFound();
        return View(booking);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager,BookingStaff")]
    public async Task<IActionResult> GetRoomsByBoat(int boatId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdString, out int userId);
        var allowedBoatIds = await GetAllowedBoatIds(userId);

        if (!allowedBoatIds.Contains(boatId))
            return Json(new List<object>());

        var rooms = await _db.Rooms
            .Where(r => r.BoatId == boatId)
            .OrderBy(r => r.RoomCode)
            .Select(r => new { r.Id, r.RoomCode, r.RoomName, r.TotalRooms })
            .ToListAsync();
        return Json(rooms);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager,BookingStaff")]
    public async Task<IActionResult> Delete(int id, Booking booking)
    {
        var b = await _db.Bookings.FindAsync(id);
        if (b == null) return NotFound();
        _db.Bookings.Remove(b);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
