using System.Globalization;
using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Services;

public record ImportResult(int Bookings, int SaleEntries, List<string> Warnings);

public class ExcelImportService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ExcelImportService> _log;

    public ExcelImportService(AppDbContext db, ILogger<ExcelImportService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<ImportResult> ImportAllAsync(string bookingFile, string saleFile, CancellationToken ct = default)
    {
        var warnings = new List<string>();
        warnings.Add("Excel import is disabled after schema refactoring. New schema requires manual data entry or API integration.");
        return new ImportResult(0, 0, warnings);
    }

    public List<Booking> ImportBookings(string path, List<string> warnings)
    {
        warnings.Add("Booking import disabled after schema refactoring");
        return new List<Booking>();
    }

    public List<SaleEntry> ImportSaleEntries(string path, List<string> warnings)
    {
        warnings.Add("Sale entries import disabled after schema refactoring");
        return new List<SaleEntry>();
    }

    private static (int year, int month) ParseSheetMonth(string name)
    {
        // Expect "T<m> - <yyyy>"
        var parts = name.Split('-', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2) return (0, 0);
        var left = parts[0];
        if (!left.StartsWith('T') && !left.StartsWith('t')) return (0, 0);
        if (!int.TryParse(left[1..], out var month)) return (0, 0);
        if (!int.TryParse(parts[1], out var year)) return (0, 0);
        return (year, month);
    }

    private static decimal? TryDecimal(IXLCell c)
    {
        if (c.IsEmpty()) return null;
        if (c.DataType == XLDataType.Number) return (decimal)c.GetDouble();
        if (decimal.TryParse(c.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
        return null;
    }

    private static int? TryInt(IXLCell c)
    {
        var d = TryDecimal(c);
        return d.HasValue ? (int)d.Value : null;
    }

    private static DateOnly? TryDate(IXLCell c)
    {
        if (c.IsEmpty()) return null;
        if (c.DataType == XLDataType.DateTime) return DateOnly.FromDateTime(c.GetDateTime());
        var s = c.GetString().Trim();
        if (string.IsNullOrEmpty(s)) return null;
        // Vietnamese-style "22-thg 12-2026" → normalise "thg" to month token
        var normalised = s.Replace("thg ", string.Empty).Replace("THG ", string.Empty);
        var formats = new[]
        {
            "d-M-yyyy", "dd-MM-yyyy", "d/M/yyyy", "dd/MM/yyyy",
            "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy"
        };
        if (DateTime.TryParseExact(normalised, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return DateOnly.FromDateTime(d);
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            return DateOnly.FromDateTime(d);
        return null;
    }

    private static string? Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Length <= max ? s : s[..max];
    }
}
