using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using BookingAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class DashboardController : BaseController
{
    private readonly DashboardService _dashboardService;

    public DashboardController(AppDbContext db, DashboardService dashboardService) : base(db)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index(string period = "week")
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdString, out int userId);

        var selectedBoatId = HttpContext.Session.GetInt32("SelectedBoatId");
        if (!selectedBoatId.HasValue)
        {
            var allowedBoatIds = await GetAllowedBoatIds(userId);
            if (!allowedBoatIds.Any())
                return View(new DashboardViewModel());

            selectedBoatId = allowedBoatIds.First();
        }

        var boat = await _db.Boats.FindAsync(selectedBoatId);
        if (boat == null)
            return View(new DashboardViewModel());

        (var periodStart, var periodEnd) = GetPeriodDates(period);
        var periodLabels = _dashboardService.GetPeriodLabels(period, periodStart, periodEnd);

        var model = new DashboardViewModel
        {
            BoatId = selectedBoatId.Value,
            BoatName = boat.Name,
            Period = period,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PeriodLabels = periodLabels,
            EmployeeSalesData = await _dashboardService.GetEmployeeSalesDataAsync(selectedBoatId.Value, period, periodStart, periodEnd, periodLabels),
            EmployeeCancellationData = await _dashboardService.GetEmployeeCancellationDataAsync(selectedBoatId.Value, period, periodStart, periodEnd, periodLabels),
            ChannelSummaryData = await _dashboardService.GetChannelSummaryDataAsync(selectedBoatId.Value, periodStart, periodEnd),
            EmployeeLastminData = await _dashboardService.GetEmployeeLastminDataAsync(selectedBoatId.Value, periodStart, periodEnd),
            ChannelLastminData = await _dashboardService.GetChannelLastminDataAsync(selectedBoatId.Value, periodStart, periodEnd)
        };

        return View(model);
    }

    public async Task<IActionResult> ExportExcel(string period = "week")
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdString, out int userId);

        var selectedBoatId = HttpContext.Session.GetInt32("SelectedBoatId");
        if (!selectedBoatId.HasValue)
            return BadRequest("No boat selected");

        var boat = await _db.Boats.FindAsync(selectedBoatId);
        if (boat == null)
            return NotFound();

        (var periodStart, var periodEnd) = GetPeriodDates(period);
        var periodLabels = _dashboardService.GetPeriodLabels(period, periodStart, periodEnd);

        var employeeSalesData = await _dashboardService.GetEmployeeSalesDataAsync(selectedBoatId.Value, period, periodStart, periodEnd, periodLabels);
        var employeeCancellationData = await _dashboardService.GetEmployeeCancellationDataAsync(selectedBoatId.Value, period, periodStart, periodEnd, periodLabels);
        var channelSummaryData = await _dashboardService.GetChannelSummaryDataAsync(selectedBoatId.Value, periodStart, periodEnd);
        var employeeLastminData = await _dashboardService.GetEmployeeLastminDataAsync(selectedBoatId.Value, periodStart, periodEnd);
        var channelLastminData = await _dashboardService.GetChannelLastminDataAsync(selectedBoatId.Value, periodStart, periodEnd);

        var excelService = new ExcelExportService();
        var fileBytes = excelService.GenerateDashboardExcel(
            boat.Name,
            period,
            periodLabels,
            employeeSalesData,
            employeeCancellationData,
            channelSummaryData,
            employeeLastminData,
            channelLastminData
        );

        var fileName = $"Dashboard_{boat.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private (DateTime Start, DateTime End) GetPeriodDates(string period)
    {
        var today = DateTime.Today;

        if (period == "week")
        {
            var daysToMonday = (int)today.DayOfWeek - 1;
            if (daysToMonday < 0) daysToMonday = 6;
            var monday = today.AddDays(-daysToMonday);
            var sunday = monday.AddDays(6);
            return (monday, sunday);
        }
        else if (period == "month")
        {
            var start = new DateTime(today.Year, today.Month, 1);
            var end = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return (start, end);
        }
        else if (period == "year")
        {
            var start = new DateTime(today.Year, 1, 1);
            var end = new DateTime(today.Year, 12, 31);
            return (start, end);
        }

        return (today, today);
    }
}
