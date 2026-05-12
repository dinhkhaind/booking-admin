using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

[Authorize]
public class PackagesController : BaseController
{
    public PackagesController(AppDbContext db) : base(db) { }

    public async Task<IActionResult> Index()
    {
        var packages = await _db.Packages.OrderBy(p => p.Name).ToListAsync();
        return View(packages);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Package package)
    {
        if (!ModelState.IsValid) return View(package);
        _db.Packages.Add(package);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var package = await _db.Packages.FindAsync(id);
        if (package == null) return NotFound();
        return View(package);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Package package)
    {
        if (id != package.Id) return BadRequest();
        if (!ModelState.IsValid) return View(package);

        _db.Packages.Update(package);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var package = await _db.Packages.FindAsync(id);
        if (package == null) return NotFound();
        return View(package);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, Package package)
    {
        var p = await _db.Packages.FindAsync(id);
        if (p == null) return NotFound();
        _db.Packages.Remove(p);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
