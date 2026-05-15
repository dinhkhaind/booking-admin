using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using BookingAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingAdmin.Web.Controllers;

[Authorize]
[Route("lich-phong")]
public class RoomScheduleController : BaseController
{
    private readonly RoomScheduleService _scheduleService;

    public RoomScheduleController(AppDbContext db, RoomScheduleService scheduleService) : base(db)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int? boatId, int? month, int? year, int? roomTypeId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdString, out int userId);
        var allowedBoatIds = await GetAllowedBoatIds(userId);

        var today = DateTime.Today;
        var selectedBoatId = boatId ?? allowedBoatIds.FirstOrDefault();
        var selectedMonth = month ?? today.Month;
        var selectedYear = year ?? today.Year;

        if (selectedBoatId == 0)
            return BadRequest("No boat selected");

        if (!allowedBoatIds.Contains(selectedBoatId))
            return Forbid();

        var model = await _scheduleService.GetRoomScheduleAsync(selectedBoatId, selectedMonth, selectedYear, roomTypeId);
        return View(model);
    }

    [HttpGet("api/bookings/{id}")]
    public async Task<IActionResult> GetBookingDetail(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.BookingStatus)
            .Include(b => b.Package)
            .Include(b => b.Channel)
            .ThenInclude(c => c.ChannelType)
            .Include(b => b.Currency)
            .Include(b => b.Boat)
            .Include(b => b.BookingRooms)
            .ThenInclude(br => br.Room)
            .ThenInclude(r => r.RoomType)
            .Include(b => b.Employee)
            .Include(b => b.EnteredByUser)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
            return NotFound();

        var room = booking.BookingRooms.FirstOrDefault()?.Room;
        var roomId = room?.Id ?? 0;
        var roomTypeId = room?.RoomTypeId ?? 0;
        var nights = (booking.CheckOut.Day - booking.CheckIn.Day);
        if (nights <= 0) nights = 1;

        return Json(new
        {
            id = booking.Id,
            systemCode = booking.SystemCode,
            agencyBookingCode = booking.AgencyBookingCode,
            code = booking.AgencyBookingCode ?? $"B{booking.Id}",
            customerName = booking.CustomerName,
            checkIn = booking.CheckIn.ToString("dd/MM"),
            checkOut = booking.CheckOut.ToString("dd/MM"),
            checkInFull = booking.CheckIn.ToString("dd/MM/yyyy"),
            checkOutFull = booking.CheckOut.ToString("dd/MM/yyyy"),
            checkInInput = booking.CheckIn.ToString("yyyy-MM-dd"),
            checkOutInput = booking.CheckOut.ToString("yyyy-MM-dd"),
            entryDateInput = booking.EntryDate?.ToString("yyyy-MM-dd"),
            nights = nights,
            adults = booking.AdultCount,
            children = booking.ChildCount,
            infants = booking.InfantCount,
            totalAdults = booking.AdultCount + booking.ChildCount + booking.InfantCount,
            totalPrice = booking.TotalPrice,
            hasTransferService = booking.HasTransferService,
            pickupPoint = booking.PickupPoint,
            dropoffPoint = booking.DropoffPoint,
            note = booking.Note,
            entryDate = booking.EntryDate?.ToString("dd/MM/yyyy HH:mm"),
            boatId = booking.BoatId,
            channelId = booking.ChannelId,
            packageId = booking.PackageId,
            currencyId = booking.CurrencyId,
            roomId = roomId,
            roomTypeId = roomTypeId,
            employeeId = booking.EmployeeId,
            enteredByUserId = booking.EnteredByUserId,
            statusId = booking.StatusId,
            // Display values
            boatName = booking.Boat?.Name,
            channelName = booking.Channel?.Name,
            channelTypeName = booking.Channel?.ChannelType?.Name,
            roomCode = room?.RoomCode,
            roomName = room?.RoomName,
            roomTypeName = room?.RoomType?.Name,
            packageName = booking.Package?.Name,
            packageCode = booking.Package?.Code,
            statusName = booking.BookingStatus?.Name,
            statusCode = booking.BookingStatus?.Code,
            statusColor = booking.BookingStatus?.Color,
            currencyCode = booking.Currency?.Code,
            employeeName = booking.Employee?.FullName,
            enteredByUsername = booking.EnteredByUser?.Username,
            roomCount = booking.BookingRooms.Count
        });
    }

    [HttpPost("api/bookings")]
    public async Task<IActionResult> CreateBooking([FromBody] BookingCreateRequest request)
    {
        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);
            var allowedBoatIds = await GetAllowedBoatIds(userId);

            if (!allowedBoatIds.Contains(request.BoatId))
                return Forbid();

            var package = await _db.Packages.FindAsync(request.PackageId);
            if (package == null)
                return BadRequest(new { error = "Package not found" });

            var checkOut = request.CheckIn.AddDays(package.AddedDate + 1);

            // Validate room overlap if RoomId is provided
            if (request.RoomId > 0)
            {
                var overlappingBooking = await _db.BookingRooms
                    .Include(br => br.Booking)
                    .Where(br => br.RoomId == request.RoomId
                        && br.Booking != null
                        && br.Booking.CheckIn < checkOut
                        && br.Booking.CheckOut > request.CheckIn
                        && br.Booking.StatusId != 3) // Exclude cancelled bookings
                    .FirstOrDefaultAsync();

                if (overlappingBooking != null)
                    return BadRequest(new { error = "Phòng này đã có booking trùng lặp trong khoảng thời gian này" });
            }

            var booking = new Booking
            {
                SystemCode = GenerateSystemCode(),
                AgencyBookingCode = request.AgencyBookingCode,
                BoatId = request.BoatId,
                ChannelId = request.ChannelId,
                CustomerName = request.CustomerName,
                AdultCount = request.AdultCount,
                ChildCount = request.ChildCount,
                InfantCount = request.InfantCount,
                CheckIn = request.CheckIn,
                CheckOut = checkOut,
                PackageId = request.PackageId,
                TotalPrice = request.TotalPrice,
                CurrencyId = request.CurrencyId,
                HasTransferService = request.HasTransferService,
                PickupPoint = request.PickupPoint,
                DropoffPoint = request.DropoffPoint,
                Note = request.Note,
                EmployeeId = request.EmployeeId,
                EnteredByUserId = request.EnteredByUserId,
                StatusId = request.StatusId > 0 ? request.StatusId : 1,
                EntryDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            if (request.RoomId > 0)
            {
                var bookingRoom = new BookingRoom
                {
                    BookingId = booking.Id,
                    RoomId = request.RoomId,
                    Quantity = 1
                };
                _db.BookingRooms.Add(bookingRoom);
                await _db.SaveChangesAsync();
            }

            return Ok(new { id = booking.Id, message = "Booking created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("api/bookings/{id}")]
    public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingCreateRequest request)
    {
        try {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);
            var allowedBoatIds = await GetAllowedBoatIds(userId);

            if (!allowedBoatIds.Contains(request.BoatId))
                return Forbid();

            var booking = await _db.Bookings.Include(b => b.BookingRooms).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
                return NotFound();

            var package = await _db.Packages.FindAsync(request.PackageId);
            if (package == null)
                return BadRequest(new { error = "Package not found" });

            var checkOut = request.CheckIn.AddDays(package.AddedDate + 1);

            // Validate room overlap (excluding current booking)
            if (request.RoomId > 0)
            {
                var overlappingBooking = await _db.BookingRooms
                    .Include(br => br.Booking)
                    .Where(br => br.RoomId == request.RoomId
                        && br.BookingId != id  // Exclude current booking
                        && br.Booking != null
                        && br.Booking.CheckIn < checkOut
                        && br.Booking.CheckOut > request.CheckIn
                        && br.Booking.StatusId != 3) // Exclude cancelled bookings
                    .FirstOrDefaultAsync();

                if (overlappingBooking != null)
                    return BadRequest(new { error = "Phòng này đã có booking trùng lặp trong khoảng thời gian này" });
            }

            // Update booking fields
            booking.AgencyBookingCode = request.AgencyBookingCode;
            booking.BoatId = request.BoatId;
            booking.ChannelId = request.ChannelId;
            booking.CustomerName = request.CustomerName;
            booking.AdultCount = request.AdultCount;
            booking.ChildCount = request.ChildCount;
            booking.InfantCount = request.InfantCount;
            booking.CheckIn = request.CheckIn;
            booking.CheckOut = checkOut;
            booking.PackageId = request.PackageId;
            booking.TotalPrice = request.TotalPrice;
            booking.CurrencyId = request.CurrencyId;
            booking.HasTransferService = request.HasTransferService;
            booking.PickupPoint = request.PickupPoint;
            booking.DropoffPoint = request.DropoffPoint;
            booking.Note = request.Note;
            booking.EmployeeId = request.EmployeeId;
            booking.EnteredByUserId = request.EnteredByUserId;
            booking.StatusId = request.StatusId > 0 ? request.StatusId : 1;

            _db.Bookings.Update(booking);
            await _db.SaveChangesAsync();

            // Update rooms if changed
            if (request.RoomId > 0)
            {
                var existingRooms = booking.BookingRooms.ToList();
                _db.BookingRooms.RemoveRange(existingRooms);

                var bookingRoom = new BookingRoom
                {
                    BookingId = booking.Id,
                    RoomId = request.RoomId,
                    Quantity = 1
                };
                _db.BookingRooms.Add(bookingRoom);
                await _db.SaveChangesAsync();
            }

            return Ok(new { id = booking.Id, message = "Booking updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("api/bookings/{id}/cancel")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        try
        {
            var booking = await _db.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            // Set status to Cancelled (id = 3)
            booking.StatusId = 3;
            _db.Bookings.Update(booking);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Booking cancelled successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private string GenerateSystemCode()
    {
        var now = DateTime.UtcNow;
        var ymd = now.ToString("yyyyMMdd");
        var rand = Random.Shared.Next(100000, 999999);
        return $"SYS-{ymd}-{rand}";
    }

    [HttpPost("api/room-blocks")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateRoomBlock([FromBody] CreateRoomBlockRequest request)
    {
        try
        {
            // Validate input
            if (request == null)
                return Json(new { success = false, error = "Request body is required" });

            if (request.RoomIds == null || request.RoomIds.Count == 0 || request.BoatId <= 0)
                return Json(new { success = false, error = "Invalid room or boat" });

            if (string.IsNullOrWhiteSpace(request.FromDate) || string.IsNullOrWhiteSpace(request.ToDate))
                return Json(new { success = false, error = "Dates are required" });

            if (!DateOnly.TryParse(request.FromDate, out var fromDate))
                return Json(new { success = false, error = "Invalid FromDate format (use YYYY-MM-DD)" });

            if (!DateOnly.TryParse(request.ToDate, out var toDate))
                return Json(new { success = false, error = "Invalid ToDate format (use YYYY-MM-DD)" });

            if (toDate <= fromDate)
                return Json(new { success = false, error = "ToDate must be after FromDate" });

            // Parse deadline if provided and convert to UTC
            DateTime? deadline = null;
            if (!string.IsNullOrWhiteSpace(request.Deadline))
            {
                if (DateTime.TryParse(request.Deadline, out var parsedDeadline))
                    deadline = DateTime.SpecifyKind(parsedDeadline, DateTimeKind.Utc);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

            var roomBlocks = new List<RoomBlock>();

            // Create a block for each selected room
            foreach (var roomId in request.RoomIds)
            {
                // Check for overlapping bookings
                var hasBookingConflict = await _db.BookingRooms
                    .Include(br => br.Booking)
                    .Where(br => br.RoomId == roomId
                        && br.Booking.StatusId != 3
                        && br.Booking.CheckIn.CompareTo(toDate) < 0
                        && br.Booking.CheckOut.CompareTo(fromDate) > 0)
                    .AnyAsync();

                if (hasBookingConflict)
                    return Json(new { success = false, error = $"Phòng {roomId} đã có booking trong khoảng thời gian này" });

                // Check for overlapping blocks
                var hasBlockConflict = await _db.RoomBlocks
                    .Where(rb => rb.RoomId == roomId
                        && rb.IsActive
                        && rb.FromDate < toDate
                        && rb.ToDate > fromDate)
                    .AnyAsync();

                if (hasBlockConflict)
                    return Json(new { success = false, error = $"Phòng {roomId} đã bị block trong khoảng thời gian này" });

                var roomBlock = new RoomBlock
                {
                    BoatId = request.BoatId,
                    RoomId = roomId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Partner = request.Partner,
                    Deadline = deadline,
                    Note = request.Note,
                    CreatedByUserId = userId > 0 ? userId : null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                roomBlocks.Add(roomBlock);
            }

            _db.RoomBlocks.AddRange(roomBlocks);
            await _db.SaveChangesAsync();

            return Json(new { success = true, count = roomBlocks.Count, message = $"Block {roomBlocks.Count} phòng thành công" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = "Lỗi: " + ex.Message });
        }
    }

    [HttpDelete("api/room-blocks/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteRoomBlock(int id)
    {
        try
        {
            var block = await _db.RoomBlocks.FindAsync(id);
            if (block == null)
                return Json(new { success = false, error = "Block không tồn tại" });

            block.IsActive = false;
            _db.RoomBlocks.Update(block);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Xoá block thành công" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = "Lỗi: " + ex.Message });
        }
    }

    [HttpGet("api/room-blocks/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoomBlockDetail(int id)
    {
        try
        {
            var block = await _db.RoomBlocks
                .Include(rb => rb.Room)
                .Include(rb => rb.CreatedByUser)
                .FirstOrDefaultAsync(rb => rb.Id == id && rb.IsActive);

            if (block == null)
                return Json(new { success = false, error = "Block không tồn tại" });

            return Json(new
            {
                success = true,
                id = block.Id,
                roomCode = block.Room?.RoomCode,
                roomName = block.Room?.RoomName,
                fromDate = block.FromDate.ToString("dd/MM/yyyy"),
                toDate = block.ToDate.ToString("dd/MM/yyyy"),
                partner = block.Partner,
                deadline = block.Deadline?.ToString("dd/MM/yyyy HH:mm"),
                note = block.Note,
                createdBy = block.CreatedByUser?.Username,
                createdAt = block.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = "Lỗi: " + ex.Message });
        }
    }

    public class CreateRoomBlockRequest
    {
        public int BoatId { get; set; }
        public List<int> RoomIds { get; set; } = new();
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Partner { get; set; }
        public string Deadline { get; set; }
        public string Note { get; set; }
    }
}
