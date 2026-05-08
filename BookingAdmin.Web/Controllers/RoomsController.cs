using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin,Manager,BookingStaff,Viewer")]
public class RoomsController : Controller
{
    private readonly AppDbContext _db;

    public RoomsController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var rooms = await _db.Rooms.OrderBy(r => r.RoomCode).ToListAsync();
        return View(rooms);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Room room)
    {
        if (!ModelState.IsValid) return View(room);
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        ViewBag.Boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Room room)
    {
        if (id != room.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.Boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
            return View(room);
        }

        _db.Rooms.Update(room);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, Room room)
    {
        var r = await _db.Rooms.FindAsync(id);
        if (r == null) return NotFound();
        _db.Rooms.Remove(r);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
