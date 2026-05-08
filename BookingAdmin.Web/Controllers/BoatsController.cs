using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin")]
public class BoatsController : Controller
{
    private readonly AppDbContext _db;

    public BoatsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var boats = await _db.Boats.OrderBy(b => b.Name).ToListAsync();
        return View(boats);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Boat boat)
    {
        if (!ModelState.IsValid) return View(boat);
        _db.Boats.Add(boat);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var boat = await _db.Boats.FindAsync(id);
        if (boat == null) return NotFound();
        return View(boat);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Boat boat)
    {
        if (id != boat.Id) return BadRequest();
        if (!ModelState.IsValid) return View(boat);

        _db.Boats.Update(boat);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var boat = await _db.Boats.FindAsync(id);
        if (boat == null) return NotFound();
        return View(boat);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, Boat boat)
    {
        var b = await _db.Boats.FindAsync(id);
        if (b == null) return NotFound();
        _db.Boats.Remove(b);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
