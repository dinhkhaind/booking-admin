using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BookingAdmin.Web.Models;
using BookingAdmin.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BookingAdmin.Web.Controllers;

[Authorize]
public class HomeController : BaseController
{
    public HomeController(AppDbContext db) : base(db) { }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetBoats()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out int userId))
            return Json(new List<object>());

        var allowedBoatIds = await GetAllowedBoatIds(userId);

        var boats = await _db.Boats
            .Where(b => b.IsActive && allowedBoatIds.Contains(b.Id))
            .OrderBy(b => b.Name)
            .Select(b => new { b.Id, b.Name })
            .ToListAsync();
        return Json(boats);
    }

    [HttpPost]
    public IActionResult SetSelectedBoat(int boatId)
    {
        HttpContext.Session.SetInt32("SelectedBoatId", boatId);
        return Ok();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
