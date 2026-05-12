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
    private const int PageSize = 20;

    public BookingsController(AppDbContext db) : base(db) { }

    [AllowAnonymous]
    public async Task<IActionResult> Index(
        int? boatId, int? month, int? year,
        int? roomTypeId, int? statusId, string? q, int page = 1)
    {
        var query = _db.Bookings.AsNoTracking()
            .Include(b => b.Boat)
            .Include(b => b.Channel)
            .Include(b => b.Package)
            .Include(b => b.Currency)
            .Include(b => b.BookingStatus)
            .Include(b => b.BookingRooms).ThenInclude(br => br.Room)
            .AsQueryable();

        // Apply filters
        if (boatId.HasValue)
            query = query.Where(b => b.BoatId == boatId.Value);
        if (month.HasValue)
            query = query.Where(b => b.CheckIn.Month == month.Value);
        if (year.HasValue)
            query = query.Where(b => b.CheckIn.Year == year.Value);
        if (roomTypeId.HasValue)
            query = query.Where(b => b.BookingRooms.Any(br =>
                br.Room != null && br.Room.RoomTypeId == roomTypeId.Value));
        if (statusId.HasValue)
            query = query.Where(b => b.StatusId == statusId.Value);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q.Trim()}%";
            query = query.Where(b =>
                EF.Functions.Like(b.AgencyBookingCode ?? string.Empty, like) ||
                EF.Functions.Like(b.CustomerName ?? string.Empty, like) ||
                EF.Functions.Like(b.Note ?? string.Empty, like));
        }

        // Count and paginate
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CheckIn).ThenBy(b => b.Boat!.Name)
            .Skip((page - 1) * PageSize).Take(PageSize)
            .ToListAsync();

        // Load dropdown lists
        var boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        var roomTypes = await _db.RoomTypes.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
        var statuses = await _db.BookingStatuses.OrderBy(s => s.SortOrder).ToListAsync();

        // Load modal dropdown data
        var channels = await _db.Channels.OrderBy(c => c.Name).ToListAsync();
        var currencies = await _db.Currencies.OrderBy(c => c.Code).ToListAsync();
        var packages = await _db.Packages.Where(p => p.IsActive).OrderBy(p => p.Code).ToListAsync();

        // Calculate revenue by currency for all bookings matching the filters (not just current page)
        var revenueByCurrencyDict = await query
            .Include(b => b.Currency)
            .GroupBy(b => b.Currency!.Code)
            .Where(g => g.Sum(b => b.TotalPrice) > 0)
            .OrderBy(g => g.Key)
            .Select(g => new { Currency = g.Key, Revenue = g.Sum(b => b.TotalPrice) })
            .ToListAsync();

        var revenueByCurrencyStr = string.Join(" | ", revenueByCurrencyDict
            .Select(kvp => $"{kvp.Currency}: {kvp.Revenue:N0}"));

        // Compose view model
        var vm = new BookingListViewModel
        {
            BoatId = boatId,
            Month = month,
            Year = year,
            RoomTypeId = roomTypeId,
            StatusId = statusId,
            Q = q,
            Page = page,
            PageSize = PageSize,
            TotalCount = total,
            Bookings = items,
            RevenueByCurrency = revenueByCurrencyStr,
            Boats = boats,
            RoomTypes = roomTypes,
            Statuses = statuses,
            ModalData = new RoomScheduleViewModel
            {
                SelectedBoatId = boatId ?? boats.FirstOrDefault()?.Id ?? 0,
                SelectedMonth = month ?? DateTime.Today.Month,
                SelectedYear = year ?? DateTime.Today.Year,
                Boats = boats,
                Channels = channels,
                Currencies = currencies,
                Packages = packages,
                Statuses = statuses
            }
        };

        return View(vm);
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
            .Include(b => b.BookingStatus)
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
        ViewBag.Statuses = await _db.BookingStatuses.OrderBy(s => s.SortOrder).ToListAsync();
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
            ViewBag.Statuses = await _db.BookingStatuses.OrderBy(s => s.SortOrder).ToListAsync();
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = string.Join(" | ", errors);
            return View(booking);
        }

        // Set default status to Pending (Id = 1) if not selected
        if (booking.StatusId == 0)
            booking.StatusId = 2;

        // Validate room overlap
        if (roomQuantities != null && roomQuantities.Any(rq => rq.Quantity > 0))
        {
            var roomIds = roomQuantities.Where(rq => rq.Quantity > 0).Select(rq => rq.RoomId).ToList();
            var overlappingBooking = await _db.BookingRooms
                .Include(br => br.Booking)
                .Where(br => roomIds.Contains(br.RoomId)
                    && br.Booking != null
                    && br.Booking.CheckIn < booking.CheckOut
                    && br.Booking.CheckOut > booking.CheckIn
                    && br.Booking.StatusId != 3) // Exclude cancelled bookings
                .FirstOrDefaultAsync();

            if (overlappingBooking != null)
            {
                TempData["Error"] = "Phòng này đã có booking trùng lặp trong khoảng thời gian này";
                ViewBag.Boats = await _db.Boats
                    .Where(b => b.IsActive && allowedBoatIds.Contains(b.Id))
                    .OrderBy(b => b.Name)
                    .ToListAsync();
                ViewBag.Channels = await _db.Channels.OrderBy(c => c.Name).ToListAsync();
                ViewBag.Employees = await _db.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
                ViewBag.Currencies = await _db.Currencies.OrderBy(c => c.Code).ToListAsync();
                ViewBag.Statuses = await _db.BookingStatuses.OrderBy(s => s.SortOrder).ToListAsync();
                return View(booking);
            }
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

        TempData["Success"] = $"Booking '{booking.AgencyBookingCode}' created successfully!";
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
        ViewBag.Statuses = await _db.BookingStatuses.OrderBy(s => s.SortOrder).ToListAsync();
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
            ViewBag.Statuses = await _db.BookingStatuses.OrderBy(s => s.SortOrder).ToListAsync();
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = string.Join(" | ", errors);
            return View(booking);
        }

        // Validate room overlap (excluding current booking)
        if (roomQuantities != null && roomQuantities.Any(rq => rq.Quantity > 0))
        {
            var roomIds = roomQuantities.Where(rq => rq.Quantity > 0).Select(rq => rq.RoomId).ToList();
            var overlappingBooking = await _db.BookingRooms
                .Include(br => br.Booking)
                .Where(br => roomIds.Contains(br.RoomId)
                    && br.BookingId != id  // Exclude current booking
                    && br.Booking != null
                    && br.Booking.CheckIn < booking.CheckOut
                    && br.Booking.CheckOut > booking.CheckIn
                    && br.Booking.StatusId != 3) // Exclude cancelled bookings
                .FirstOrDefaultAsync();

            if (overlappingBooking != null)
            {
                TempData["Error"] = "Phòng này đã có booking trùng lặp trong khoảng thời gian này";
                ViewBag.Boats = await _db.Boats
                    .Where(b => b.IsActive && allowedBoatIds.Contains(b.Id))
                    .OrderBy(b => b.Name)
                    .ToListAsync();
                ViewBag.Channels = await _db.Channels.OrderBy(c => c.Name).ToListAsync();
                ViewBag.Employees = await _db.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
                ViewBag.Currencies = await _db.Currencies.OrderBy(c => c.Code).ToListAsync();
                ViewBag.Statuses = await _db.BookingStatuses.OrderBy(s => s.SortOrder).ToListAsync();
                return View(booking);
            }
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
        TempData["Success"] = $"Booking '{booking.AgencyBookingCode}' updated successfully!";
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
            .Include(r => r.RoomType)
            .Where(r => r.BoatId == boatId)
            .OrderBy(r => r.RoomCode)
            .Select(r => new { r.Id, r.RoomCode, RoomTypeName = r.RoomType != null ? (string)r.RoomType.Name : "" })
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
