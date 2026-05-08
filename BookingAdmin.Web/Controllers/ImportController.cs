using BookingAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Manager")] // Import only for Admin/Manager
public class ImportController : Controller
{
    private readonly ExcelImportService _importer;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public ImportController(ExcelImportService importer, IConfiguration config, IWebHostEnvironment env)
    {
        _importer = importer;
        _config = config;
        _env = env;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(string? bookingPath, string? salePath)
    {
        var bp = ResolvePath(bookingPath ?? _config["ExcelImport:BookingFile"] ?? string.Empty);
        var sp = ResolvePath(salePath ?? _config["ExcelImport:SaleFile"] ?? string.Empty);

        var result = await _importer.ImportAllAsync(bp, sp);
        TempData["ImportResult"] = $"Imported {result.Bookings} booking cells, {result.SaleEntries} sale rows. Warnings: {string.Join(" | ", result.Warnings)}";
        return RedirectToAction(nameof(Index));
    }

    private string ResolvePath(string p)
    {
        if (Path.IsPathRooted(p)) return p;
        return Path.GetFullPath(Path.Combine(_env.ContentRootPath, p));
    }
}
