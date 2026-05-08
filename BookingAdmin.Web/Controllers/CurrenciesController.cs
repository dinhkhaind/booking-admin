using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CurrenciesController : Controller
{
    private readonly AppDbContext _db;

    public CurrenciesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var currencies = await _db.Currencies.OrderBy(c => c.Code).ToListAsync();
        return View(currencies);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Currency currency)
    {
        if (!ModelState.IsValid) return View(currency);
        _db.Currencies.Add(currency);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var currency = await _db.Currencies.FindAsync(id);
        if (currency == null) return NotFound();
        return View(currency);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Currency currency)
    {
        if (id != currency.Id) return BadRequest();
        if (!ModelState.IsValid) return View(currency);

        _db.Currencies.Update(currency);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var currency = await _db.Currencies.FindAsync(id);
        if (currency == null) return NotFound();
        return View(currency);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, Currency currency)
    {
        var c = await _db.Currencies.FindAsync(id);
        if (c == null) return NotFound();
        _db.Currencies.Remove(c);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
