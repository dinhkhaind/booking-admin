using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

[Authorize]
public class RoomTypesController : BaseController
{
    public RoomTypesController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        var roomTypes = await _db.RoomTypes.OrderBy(r => r.Name).ToListAsync();
        return View(roomTypes);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomType roomType)
    {
        if (!ModelState.IsValid) return View(roomType);
        _db.RoomTypes.Add(roomType);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var roomType = await _db.RoomTypes.FindAsync(id);
        if (roomType == null) return NotFound();
        return View(roomType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoomType roomType)
    {
        if (id != roomType.Id) return BadRequest();
        if (!ModelState.IsValid) return View(roomType);

        _db.RoomTypes.Update(roomType);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var roomType = await _db.RoomTypes.FindAsync(id);
        if (roomType == null) return NotFound();
        return View(roomType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, RoomType roomType)
    {
        var rt = await _db.RoomTypes.FindAsync(id);
        if (rt == null) return NotFound();
        _db.RoomTypes.Remove(rt);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("api/roomtypes")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoomTypesByBoat(int? boatId)
    {
        var query = _db.RoomTypes.Where(rt => rt.IsActive);

        if (boatId.HasValue && boatId.Value > 0)
            query = query.Where(rt => rt.Rooms.Any(r => r.BoatId == boatId.Value));

        var roomTypes = await query
            .OrderBy(rt => rt.Name)
            .Select(rt => new { rt.Id, rt.Name })
            .ToListAsync();

        return Json(roomTypes);
    }
}
