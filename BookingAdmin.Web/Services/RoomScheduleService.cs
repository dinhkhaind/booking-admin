using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Services;

public class RoomScheduleService
{
    private readonly AppDbContext _db;
    private const int CancelledStatusId = 3;

    public RoomScheduleService(AppDbContext db) => _db = db;

    public async Task<RoomScheduleViewModel> GetRoomScheduleAsync(int boatId, int month, int year, int? roomTypeId)
    {
        var monthStart = new DateOnly(year, month, 1);
        var monthEnd = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        var daysInMonth = monthEnd.Day;

        var boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        var roomTypes = await _db.RoomTypes.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
        var channels = await _db.Channels.OrderBy(c => c.Name).ToListAsync();
        var currencies = await _db.Currencies.OrderBy(c => c.Code).ToListAsync();
        var packages = await _db.Packages.Where(p => p.IsActive).OrderBy(p => p.Code).ToListAsync();
        var statuses = await _db.BookingStatuses.OrderBy(s => s.SortOrder).ToListAsync();

        var roomsQuery = _db.Rooms
            .Include(r => r.RoomType)
            .Where(r => r.BoatId == boatId && r.IsActive);

        if (roomTypeId.HasValue)
            roomsQuery = roomsQuery.Where(r => r.RoomTypeId == roomTypeId.Value);

        var rooms = await roomsQuery
            .OrderBy(r => r.RoomCode)
            .ToListAsync();

        var bookingRooms = await _db.BookingRooms
            .Include(br => br.Booking)
            .ThenInclude(b => b.BookingStatus)
            .Include(br => br.Booking)
            .ThenInclude(b => b.Package)
            .Include(br => br.Booking)
            .ThenInclude(b => b.Channel)
            .Where(br => rooms.Select(r => r.Id).Contains(br.RoomId)
                && br.Booking.CheckIn <= monthEnd
                && br.Booking.CheckOut > monthStart
                && br.Booking.StatusId != CancelledStatusId)
            .ToListAsync();

        var roomBlocks = await _db.RoomBlocks
            .Where(rb => rooms.Select(r => r.Id).Contains(rb.RoomId)
                && rb.IsActive
                && rb.FromDate <= monthEnd
                && rb.ToDate > monthStart)
            .ToListAsync();

        var rows = new List<RoomScheduleRow>();
        foreach (var room in rooms)
        {
            var row = new RoomScheduleRow
            {
                RoomId = room.Id,
                RoomCode = room.RoomCode,
                RoomName = room.RoomName ?? room.RoomCode,
                RoomTypeName = room.RoomType?.Name ?? "Unknown",
                Location = room.Location ?? ""
            };

            var occupancyMap = new Dictionary<int, (object chip, bool isStart, int span, bool isBlock)>();

            var roomBookings = bookingRooms
                .Where(br => br.RoomId == room.Id)
                .GroupBy(br => br.BookingId)
                .Select(g => g.First())
                .ToList();

            foreach (var bookingRoom in roomBookings)
            {
                var booking = bookingRoom.Booking;
                if (booking == null) continue;

                var span = booking.Package?.Code == "3N2D" ? 2 : 1;
                var startDay = Math.Max(booking.CheckIn.Day, monthStart.Day);

                if (startDay <= daysInMonth)
                {
                    var chip = new BookingChipVm
                    {
                        BookingId = booking.Id,
                        Code = booking.AgencyBookingCode ?? $"B{booking.Id}",
                        CustomerName = booking.CustomerName ?? "",
                        ChannelName = booking.Channel?.Name ?? "Unknown",
                        PackageCode = booking.Package?.Code ?? "Unknown",
                        PackageAddedDate = booking.Package?.AddedDate ?? 0,
                        StatusName = booking.BookingStatus?.Name ?? "Unknown",
                        StatusColor = booking.BookingStatus?.Color ?? "#6c757d"
                    };

                    occupancyMap[startDay] = (chip, true, span, false);

                    if (span == 2 && startDay + 1 <= daysInMonth)
                        occupancyMap[startDay + 1] = (chip, false, span, false);
                }
            }

            // Add room blocks to occupancy map
            var roomBlocksList = roomBlocks.Where(rb => rb.RoomId == room.Id).ToList();
            foreach (var block in roomBlocksList)
            {
                var blockFromDate = block.FromDate;
                if (blockFromDate < monthStart)
                    blockFromDate = monthStart;

                var blockToDate = block.ToDate;
                if (blockToDate > monthEnd)
                    blockToDate = monthEnd;

                var startDay = blockFromDate.Day;
                var endDay = blockToDate.AddDays(-1).Day; // Don't color the ToDate day
                var span = endDay - startDay + 1;

                if (startDay <= daysInMonth && span > 0)
                {
                    var blockChip = new BlockChipVm
                    {
                        BlockId = block.Id,
                        Partner = block.Partner ?? "Block",
                        Note = block.Note ?? ""
                    };

                    occupancyMap[startDay] = (blockChip, true, span, true);

                    // Mark continuation days (for display purposes, though we use ColSpan)
                    for (int day = startDay + 1; day <= endDay && day <= daysInMonth; day++)
                    {
                        if (!occupancyMap.ContainsKey(day))
                            occupancyMap[day] = (blockChip, false, span, true);
                    }
                }
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                if (occupancyMap.TryGetValue(day, out var entry))
                {
                    if (entry.isStart)
                    {
                        row.Days.Add(new DayCell
                        {
                            Day = day,
                            IsStart = true,
                            ColSpan = entry.span,
                            Booking = entry.isBlock ? null : entry.chip as BookingChipVm,
                            Block = entry.isBlock ? entry.chip as BlockChipVm : null
                        });
                    }
                }
                else
                {
                    row.Days.Add(new DayCell
                    {
                        Day = day,
                        IsStart = true,
                        ColSpan = 1,
                        Booking = null,
                        Block = null
                    });
                }
            }

            rows.Add(row);
        }

        var metrics = await GetMetricsAsync(boatId, monthStart, monthEnd, bookingRooms);

        return new RoomScheduleViewModel
        {
            SelectedBoatId = boatId,
            SelectedMonth = month,
            SelectedYear = year,
            SelectedRoomTypeId = roomTypeId,
            OccupancyRate = metrics.occupancyRate,
            TotalRevenue = metrics.revenue,
            ActiveBookingCount = metrics.activeCount,
            LastMinuteBookingCount = metrics.lastMinCount,
            RevenueByCurrency = metrics.revenueByCurrency,
            Rows = rows,
            DaysInMonth = daysInMonth,
            Boats = boats,
            RoomTypes = roomTypes,
            Channels = channels,
            Currencies = currencies,
            Packages = packages,
            Statuses = statuses
        };
    }

    private async Task<(double occupancyRate, decimal revenue, int activeCount, int lastMinCount, string revenueByCurrency)> GetMetricsAsync(
        int boatId, DateOnly monthStart, DateOnly monthEnd, List<BookingRoom> bookingRooms)
    {
        var activeBookings = await _db.Bookings
            .Where(b => b.BoatId == boatId
                && b.CheckIn >= monthStart
                && b.CheckIn <= monthEnd
                && b.StatusId != CancelledStatusId)
            .Include(b => b.BookingRooms)
            .Include(b => b.Currency)
            .ToListAsync();

        var totalRoomDays = await _db.Rooms
            .Where(r => r.BoatId == boatId && r.IsActive)
            .CountAsync() * monthEnd.Day;

        var occupiedRoomDays = bookingRooms.Sum(br => br.Quantity);
        var occupancyRate = totalRoomDays > 0 ? (occupiedRoomDays * 100.0) / totalRoomDays : 0;

        var revenue = activeBookings.Sum(b => b.TotalPrice);

        // Calculate revenue by currency
        var revenueByCurrencyDict = activeBookings
            .GroupBy(b => b.Currency?.Code ?? "Unknown")
            .Where(g => g.Sum(b => b.TotalPrice) > 0)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(b => b.TotalPrice)
            )
            .OrderBy(kvp => kvp.Key)
            .ToList();

        var revenueByCurrencyStr = string.Join(" | ", revenueByCurrencyDict
            .Select(kvp => $"{kvp.Key}: {kvp.Value:N0}"));

        var lastMinBookings = activeBookings
            .Where(b => (b.CheckIn.ToDateTime(TimeOnly.MinValue) - b.CreatedAt).TotalDays < 3)
            .Count();

        return (occupancyRate, revenue, activeBookings.Count, lastMinBookings, revenueByCurrencyStr);
    }
}
