using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Services;

public class DashboardService
{
    private readonly AppDbContext _db;
    private const int CancelledStatusId = 3; // Seeded as "Cancelled"

    public DashboardService(AppDbContext db) => _db = db;

    public List<string> GetPeriodLabels(string period, DateTime periodStart, DateTime periodEnd)
    {
        var labels = new List<string>();

        if (period == "week")
        {
            var current = periodStart;
            string[] days = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật" };
            while (current <= periodEnd)
            {
                int dayOfWeek = (int)current.DayOfWeek;
                if (dayOfWeek == 0) dayOfWeek = 7;
                else if (dayOfWeek > 1) dayOfWeek -= 1;

                labels.Add($"{days[dayOfWeek - 1]}\n{current:d/M}");
                current = current.AddDays(1);
            }
        }
        else if (period == "month")
        {
            var weeks = GetWeeksInMonth(periodStart);
            for (int i = 1; i <= weeks; i++)
            {
                labels.Add($"Tuần {i}");
            }
        }
        else if (period == "year")
        {
            for (int i = 1; i <= 12; i++)
            {
                labels.Add($"T{i}");
            }
        }
        else if (period == "custom")
        {
            // For custom date range, show 7-day periods
            var current = periodStart;
            int weekNum = 1;
            while (current <= periodEnd)
            {
                var weekEnd = current.AddDays(6);
                if (weekEnd > periodEnd) weekEnd = periodEnd;
                labels.Add($"{current:dd/M} - {weekEnd:dd/M}");
                current = weekEnd.AddDays(1);
                weekNum++;
            }
        }

        return labels;
    }

    private int GetWeeksInMonth(DateTime date)
    {
        var firstDay = new DateTime(date.Year, date.Month, 1);
        var lastDay = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        var firstMonday = firstDay.AddDays((8 - (int)firstDay.DayOfWeek) % 7);
        if (firstMonday > lastDay) return 1;
        return (int)Math.Ceiling((lastDay - firstMonday).TotalDays / 7.0) + 1;
    }

    public async Task<List<EmployeeSalesRow>> GetEmployeeSalesDataAsync(int boatId, string period, DateTime periodStart, DateTime periodEnd, List<string> periodLabels)
    {
        var periodStartDate = new DateOnly(periodStart.Year, periodStart.Month, periodStart.Day);
        var periodEndDate = new DateOnly(periodEnd.Year, periodEnd.Month, periodEnd.Day);

        var bookings = await _db.Bookings
            .Include(b => b.BookingRooms)
            .Include(b => b.Currency)
            .Where(b => b.BoatId == boatId && b.CheckIn >= periodStartDate && b.CheckIn <= periodEndDate && b.StatusId != CancelledStatusId)
            .ToListAsync();

        var employees = await _db.Employees.Where(e => e.IsActive).ToListAsync();
        var result = new List<EmployeeSalesRow>();

        foreach (var emp in employees)
        {
            var empBookings = bookings.Where(b => b.EmployeeId == emp.Id).ToList();
            if (!empBookings.Any()) continue;

            var row = new EmployeeSalesRow { EmployeeName = emp.FullName };

            foreach (var label in periodLabels)
            {
                var periodBookings = GetPeriodBookings(empBookings, period, label, periodStart);

                // Group by currency
                var currencyBreakdown = periodBookings
                    .GroupBy(b => b.Currency!.Code)
                    .ToDictionary(g => g.Key, g => g.Sum(b => b.BookingRooms.Sum(br => br.Quantity * b.TotalPrice)));

                row.PeriodSales[label] = currencyBreakdown;

                // Calculate total and accumulate to TotalByCurrency
                foreach (var kvp in currencyBreakdown)
                {
                    row.Total += kvp.Value;
                    if (!row.TotalByCurrency.ContainsKey(kvp.Key))
                        row.TotalByCurrency[kvp.Key] = 0;
                    row.TotalByCurrency[kvp.Key] += kvp.Value;
                }
            }

            result.Add(row);
        }

        // Add total row
        if (result.Any())
        {
            var totalRow = new EmployeeSalesRow { EmployeeName = "TỔNG CỘNG" };
            foreach (var label in periodLabels)
            {
                var combined = new Dictionary<string, decimal>();
                foreach (var row in result)
                {
                    if (row.PeriodSales.ContainsKey(label))
                    {
                        foreach (var kvp in row.PeriodSales[label])
                        {
                            if (!combined.ContainsKey(kvp.Key))
                                combined[kvp.Key] = 0;
                            combined[kvp.Key] += kvp.Value;
                        }
                    }
                }
                totalRow.PeriodSales[label] = combined;

                foreach (var kvp in combined)
                {
                    totalRow.Total += kvp.Value;
                    if (!totalRow.TotalByCurrency.ContainsKey(kvp.Key))
                        totalRow.TotalByCurrency[kvp.Key] = 0;
                    totalRow.TotalByCurrency[kvp.Key] += kvp.Value;
                }
            }
            result.Add(totalRow);
        }

        return result;
    }

    private List<Booking> GetPeriodBookings(List<Booking> bookings, string period, string label, DateTime periodStart)
    {
        if (period == "week")
        {
            var dayMatch = ExtractDateFromWeekLabel(label, periodStart);
            var dayMatchDate = new DateOnly(dayMatch.Year, dayMatch.Month, dayMatch.Day);
            return bookings.Where(b => b.CheckIn == dayMatchDate).ToList();
        }
        else if (period == "month")
        {
            var weekNum = int.Parse(label.Replace("Tuần ", ""));
            var weekDates = GetWeekDatesInMonth(periodStart, weekNum);
            var weekStartDate = new DateOnly(weekDates.Start.Year, weekDates.Start.Month, weekDates.Start.Day);
            var weekEndDate = new DateOnly(weekDates.End.Year, weekDates.End.Month, weekDates.End.Day);
            return bookings.Where(b => b.CheckIn >= weekStartDate && b.CheckIn <= weekEndDate).ToList();
        }
        else if (period == "year")
        {
            var monthNum = int.Parse(label.Replace("T", ""));
            return bookings.Where(b => b.CheckIn.Month == monthNum).ToList();
        }
        else if (period == "custom")
        {
            // For custom period, split label to get date range
            var parts = label.Split(" - ");
            if (parts.Length == 2 && DateOnly.TryParse(parts[0], out var start) && DateOnly.TryParse(parts[1], out var end))
            {
                return bookings.Where(b => b.CheckIn >= start && b.CheckIn <= end).ToList();
            }
        }

        return new List<Booking>();
    }

    public async Task<List<EmployeeSalesRow>> GetEmployeeCancellationDataAsync(int boatId, string period, DateTime periodStart, DateTime periodEnd, List<string> periodLabels)
    {
        var periodStartDate = new DateOnly(periodStart.Year, periodStart.Month, periodStart.Day);
        var periodEndDate = new DateOnly(periodEnd.Year, periodEnd.Month, periodEnd.Day);

        var bookings = await _db.Bookings
            .Include(b => b.BookingRooms)
            .Include(b => b.Currency)
            .Where(b => b.BoatId == boatId && b.CheckIn >= periodStartDate && b.CheckIn <= periodEndDate && b.StatusId == CancelledStatusId)
            .ToListAsync();

        var employees = await _db.Employees.Where(e => e.IsActive).ToListAsync();
        var result = new List<EmployeeSalesRow>();

        foreach (var emp in employees)
        {
            var empBookings = bookings.Where(b => b.EmployeeId == emp.Id).ToList();
            if (!empBookings.Any()) continue;

            var row = new EmployeeSalesRow { EmployeeName = emp.FullName };

            foreach (var label in periodLabels)
            {
                var periodBookings = GetPeriodBookings(empBookings, period, label, periodStart);

                // Group by currency
                var currencyBreakdown = periodBookings
                    .GroupBy(b => b.Currency!.Code)
                    .ToDictionary(g => g.Key, g => g.Sum(b => b.BookingRooms.Sum(br => br.Quantity * b.TotalPrice)));

                row.PeriodSales[label] = currencyBreakdown;

                // Calculate total and accumulate to TotalByCurrency
                foreach (var kvp in currencyBreakdown)
                {
                    row.Total += kvp.Value;
                    if (!row.TotalByCurrency.ContainsKey(kvp.Key))
                        row.TotalByCurrency[kvp.Key] = 0;
                    row.TotalByCurrency[kvp.Key] += kvp.Value;
                }
            }

            result.Add(row);
        }

        // Add total row
        if (result.Any())
        {
            var totalRow = new EmployeeSalesRow { EmployeeName = "TỔNG CỘNG" };
            foreach (var label in periodLabels)
            {
                var combined = new Dictionary<string, decimal>();
                foreach (var row in result)
                {
                    if (row.PeriodSales.ContainsKey(label))
                    {
                        foreach (var kvp in row.PeriodSales[label])
                        {
                            if (!combined.ContainsKey(kvp.Key))
                                combined[kvp.Key] = 0;
                            combined[kvp.Key] += kvp.Value;
                        }
                    }
                }
                totalRow.PeriodSales[label] = combined;

                foreach (var kvp in combined)
                {
                    totalRow.Total += kvp.Value;
                    if (!totalRow.TotalByCurrency.ContainsKey(kvp.Key))
                        totalRow.TotalByCurrency[kvp.Key] = 0;
                    totalRow.TotalByCurrency[kvp.Key] += kvp.Value;
                }
            }
            result.Add(totalRow);
        }

        return result;
    }

    public async Task<List<ChannelSummaryRow>> GetChannelSummaryDataAsync(int boatId, DateTime periodStart, DateTime periodEnd)
    {
        var periodStartDate = new DateOnly(periodStart.Year, periodStart.Month, periodStart.Day);
        var periodEndDate = new DateOnly(periodEnd.Year, periodEnd.Month, periodEnd.Day);

        var bookings = await _db.Bookings
            .Include(b => b.BookingRooms)
            .Include(b => b.Channel)
            .Include(b => b.Channel!.ChannelType)
            .Include(b => b.Currency)
            .Where(b => b.BoatId == boatId && b.CheckIn >= periodStartDate && b.CheckIn <= periodEndDate)
            .ToListAsync();

        var channelTypes = await _db.ChannelTypes.OrderBy(ct => ct.Name).ToListAsync();
        var result = new List<ChannelSummaryRow>();

        foreach (var channelType in channelTypes)
        {
            var activeBookings = bookings.Where(b => b.Channel!.ChannelTypeId == channelType.Id && b.StatusId != CancelledStatusId).ToList();
            var cancelledBookings = bookings.Where(b => b.Channel!.ChannelTypeId == channelType.Id && b.StatusId == CancelledStatusId).ToList();

            if (!activeBookings.Any() && !cancelledBookings.Any()) continue;

            var row = new ChannelSummaryRow { ChannelName = channelType.Name };
            row.TotalRooms = activeBookings.Sum(b => b.BookingRooms.Sum(br => br.Quantity));
            row.TotalSales = activeBookings.Sum(b => b.BookingRooms.Sum(br => br.Quantity * (b.TotalPrice)));
            row.TotalSalesByCurrency = activeBookings
                .GroupBy(b => b.Currency!.Code)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.BookingRooms.Sum(br => br.Quantity * b.TotalPrice)));
            row.TotalCustomers = activeBookings.Sum(b => b.AdultCount + b.ChildCount);
            row.CancelledRooms = cancelledBookings.Sum(b => b.BookingRooms.Sum(br => br.Quantity));
            row.CancelledSales = cancelledBookings.Sum(b => b.BookingRooms.Sum(br => br.Quantity * (b.TotalPrice)));
            row.CancelledSalesByCurrency = cancelledBookings
                .GroupBy(b => b.Currency!.Code)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.BookingRooms.Sum(br => br.Quantity * b.TotalPrice)));
            row.CancelledCustomers = cancelledBookings.Sum(b => b.AdultCount + b.ChildCount);

            result.Add(row);
        }

        // Add total row
        if (result.Any())
        {
            var totalRow = new ChannelSummaryRow
            {
                ChannelName = "TỔNG CỘNG",
                TotalRooms = result.Sum(r => r.TotalRooms),
                TotalSales = result.Sum(r => r.TotalSales),
                TotalSalesByCurrency = result
                    .SelectMany(r => r.TotalSalesByCurrency)
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value)),
                TotalCustomers = result.Sum(r => r.TotalCustomers),
                CancelledRooms = result.Sum(r => r.CancelledRooms),
                CancelledSales = result.Sum(r => r.CancelledSales),
                CancelledSalesByCurrency = result
                    .SelectMany(r => r.CancelledSalesByCurrency)
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value)),
                CancelledCustomers = result.Sum(r => r.CancelledCustomers)
            };
            result.Add(totalRow);
        }

        return result;
    }

    public async Task<List<EmployeeLastminRow>> GetEmployeeLastminDataAsync(int boatId, DateTime periodStart, DateTime periodEnd)
    {
        var periodStartDate = new DateOnly(periodStart.Year, periodStart.Month, periodStart.Day);
        var periodEndDate = new DateOnly(periodEnd.Year, periodEnd.Month, periodEnd.Day);

        var bookings = await _db.Bookings
            .Include(b => b.BookingRooms)
            .Include(b => b.Currency)
            .Where(b => b.BoatId == boatId && b.CheckIn >= periodStartDate && b.CheckIn <= periodEndDate && b.StatusId != CancelledStatusId)
            .ToListAsync();

        var employees = await _db.Employees.Where(e => e.IsActive).ToListAsync();
        var result = new List<EmployeeLastminRow>();

        foreach (var emp in employees)
        {
            var empBookings = bookings.Where(b => b.EmployeeId == emp.Id).ToList();

            var lastmin1Day = empBookings.Where(b => (b.CheckIn.ToDateTime(TimeOnly.MinValue) - b.CreatedAt).TotalDays < 1).ToList();
            var lastmin3Days = empBookings.Where(b => (b.CheckIn.ToDateTime(TimeOnly.MinValue) - b.CreatedAt).TotalDays < 3).ToList();

            if (!lastmin1Day.Any() && !lastmin3Days.Any()) continue;

            var row = new EmployeeLastminRow
            {
                EmployeeName = emp.FullName,
                Rooms1Day = lastmin1Day.Sum(b => b.BookingRooms.Sum(br => br.Quantity)),
                Sales1Day = lastmin1Day.Sum(b => b.BookingRooms.Sum(br => br.Quantity * (b.TotalPrice))),
                Sales1DayByCurrency = lastmin1Day
                    .GroupBy(b => b.Currency!.Code)
                    .ToDictionary(g => g.Key, g => g.Sum(b => b.BookingRooms.Sum(br => br.Quantity * b.TotalPrice))),
                Rooms3Days = lastmin3Days.Sum(b => b.BookingRooms.Sum(br => br.Quantity)),
                Sales3Days = lastmin3Days.Sum(b => b.BookingRooms.Sum(br => br.Quantity * (b.TotalPrice))),
                Sales3DaysByCurrency = lastmin3Days
                    .GroupBy(b => b.Currency!.Code)
                    .ToDictionary(g => g.Key, g => g.Sum(b => b.BookingRooms.Sum(br => br.Quantity * b.TotalPrice)))
            };
            result.Add(row);
        }

        // Add total row
        if (result.Any())
        {
            var totalRow = new EmployeeLastminRow
            {
                EmployeeName = "TỔNG CỘNG",
                Rooms1Day = result.Sum(r => r.Rooms1Day),
                Sales1Day = result.Sum(r => r.Sales1Day),
                Sales1DayByCurrency = result
                    .SelectMany(r => r.Sales1DayByCurrency)
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value)),
                Rooms3Days = result.Sum(r => r.Rooms3Days),
                Sales3Days = result.Sum(r => r.Sales3Days),
                Sales3DaysByCurrency = result
                    .SelectMany(r => r.Sales3DaysByCurrency)
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value))
            };
            result.Add(totalRow);
        }

        return result;
    }

    public async Task<List<ChannelLastminRow>> GetChannelLastminDataAsync(int boatId, DateTime periodStart, DateTime periodEnd)
    {
        var periodStartDate = new DateOnly(periodStart.Year, periodStart.Month, periodStart.Day);
        var periodEndDate = new DateOnly(periodEnd.Year, periodEnd.Month, periodEnd.Day);

        var bookings = await _db.Bookings
            .Include(b => b.BookingRooms)
            .Include(b => b.Currency)
            .Where(b => b.BoatId == boatId && b.CheckIn >= periodStartDate && b.CheckIn <= periodEndDate && b.StatusId != CancelledStatusId)
            .ToListAsync();

        var channels = await _db.Channels.OrderBy(c => c.Name).ToListAsync();
        var result = new List<ChannelLastminRow>();

        foreach (var channel in channels)
        {
            var channelBookings = bookings.Where(b => b.ChannelId == channel.Id).ToList();
            var lastmin1Day = channelBookings.Where(b => (b.CheckIn.ToDateTime(TimeOnly.MinValue) - b.CreatedAt).TotalDays < 1).ToList();
            var lastmin3Days = channelBookings.Where(b => (b.CheckIn.ToDateTime(TimeOnly.MinValue) - b.CreatedAt).TotalDays < 3).ToList();

            if (!lastmin1Day.Any() && !lastmin3Days.Any()) continue;

            var row = new ChannelLastminRow
            {
                ChannelName = channel.Name,
                Rooms1Day = lastmin1Day.Sum(b => b.BookingRooms.Sum(br => br.Quantity)),
                Sales1Day = lastmin1Day.Sum(b => b.BookingRooms.Sum(br => br.Quantity * (b.TotalPrice))),
                Sales1DayByCurrency = lastmin1Day
                    .GroupBy(b => b.Currency!.Code)
                    .ToDictionary(g => g.Key, g => g.Sum(b => b.BookingRooms.Sum(br => br.Quantity * b.TotalPrice))),
                Rooms3Days = lastmin3Days.Sum(b => b.BookingRooms.Sum(br => br.Quantity)),
                Sales3Days = lastmin3Days.Sum(b => b.BookingRooms.Sum(br => br.Quantity * (b.TotalPrice))),
                Sales3DaysByCurrency = lastmin3Days
                    .GroupBy(b => b.Currency!.Code)
                    .ToDictionary(g => g.Key, g => g.Sum(b => b.BookingRooms.Sum(br => br.Quantity * b.TotalPrice)))
            };
            result.Add(row);
        }

        // Add total row
        if (result.Any())
        {
            var totalRow = new ChannelLastminRow
            {
                ChannelName = "TỔNG CỘNG",
                Rooms1Day = result.Sum(r => r.Rooms1Day),
                Sales1Day = result.Sum(r => r.Sales1Day),
                Sales1DayByCurrency = result
                    .SelectMany(r => r.Sales1DayByCurrency)
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value)),
                Rooms3Days = result.Sum(r => r.Rooms3Days),
                Sales3Days = result.Sum(r => r.Sales3Days),
                Sales3DaysByCurrency = result
                    .SelectMany(r => r.Sales3DaysByCurrency)
                    .GroupBy(kvp => kvp.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value))
            };
            result.Add(totalRow);
        }

        return result;
    }

    private DateTime ExtractDateFromWeekLabel(string label, DateTime weekStart)
    {
        var parts = label.Split('\n');
        var dateStr = parts.Length > 1 ? parts[1] : parts[0];
        var dayNum = dateStr[0] switch
        {
            'T' when dateStr.Contains('h') && dateStr.Length > 3 && dateStr[3] == ' ' => int.Parse(dateStr.Substring(3).Split('/')[0]),
            _ => 1
        };
        return weekStart.AddDays(dayNum - 1);
    }

    private (DateTime Start, DateTime End) GetWeekDatesInMonth(DateTime month, int weekNum)
    {
        var firstDay = new DateTime(month.Year, month.Month, 1);
        var start = firstDay.AddDays(7 * (weekNum - 1));
        var end = start.AddDays(6);
        if (end.Month != month.Month) end = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
        return (start, end);
    }
}
