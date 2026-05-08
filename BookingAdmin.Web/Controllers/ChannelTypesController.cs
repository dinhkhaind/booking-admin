using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ChannelTypesController : Controller
{
    private readonly AppDbContext _db;

    public ChannelTypesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var channelTypes = await _db.ChannelTypes.OrderBy(ct => ct.Name).ToListAsync();
        return View(channelTypes);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChannelType channelType)
    {
        if (!ModelState.IsValid) return View(channelType);
        _db.ChannelTypes.Add(channelType);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var channelType = await _db.ChannelTypes.FindAsync(id);
        if (channelType == null) return NotFound();
        return View(channelType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ChannelType channelType)
    {
        if (id != channelType.Id) return BadRequest();
        if (!ModelState.IsValid) return View(channelType);

        _db.ChannelTypes.Update(channelType);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var channelType = await _db.ChannelTypes.FindAsync(id);
        if (channelType == null) return NotFound();
        return View(channelType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, ChannelType channelType)
    {
        var ct = await _db.ChannelTypes.FindAsync(id);
        if (ct == null) return NotFound();
        _db.ChannelTypes.Remove(ct);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
