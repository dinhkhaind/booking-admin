using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ChannelsController : Controller
{
    private readonly AppDbContext _db;

    public ChannelsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var channels = await _db.Channels.Include(c => c.ChannelType).OrderBy(c => c.Name).ToListAsync();
        return View(channels);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.ChannelTypes = new SelectList(await _db.ChannelTypes.OrderBy(ct => ct.Name).ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Channel channel)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ChannelTypes = new SelectList(await _db.ChannelTypes.OrderBy(ct => ct.Name).ToListAsync(), "Id", "Name");
            return View(channel);
        }
        _db.Channels.Add(channel);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var channel = await _db.Channels.FindAsync(id);
        if (channel == null) return NotFound();
        ViewBag.ChannelTypes = new SelectList(await _db.ChannelTypes.OrderBy(ct => ct.Name).ToListAsync(), "Id", "Name", channel.ChannelTypeId);
        return View(channel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Channel channel)
    {
        if (id != channel.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.ChannelTypes = new SelectList(await _db.ChannelTypes.OrderBy(ct => ct.Name).ToListAsync(), "Id", "Name", channel.ChannelTypeId);
            return View(channel);
        }

        _db.Channels.Update(channel);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var channel = await _db.Channels.Include(c => c.ChannelType).FirstOrDefaultAsync(c => c.Id == id);
        if (channel == null) return NotFound();
        return View(channel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, Channel channel)
    {
        var c = await _db.Channels.FindAsync(id);
        if (c == null) return NotFound();
        _db.Channels.Remove(c);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
