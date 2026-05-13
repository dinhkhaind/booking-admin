using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

    [HttpGet("api/channels")]
    [AllowAnonymous]
    public async Task<IActionResult> GetChannels()
    {
        var channels = await _db.Channels
            .OrderBy(c => c.Name)
            .Select(c => new { id = c.Id, name = c.Name })
            .ToListAsync();
        return Json(channels);
    }

    [HttpGet("api/channeltypes")]
    [AllowAnonymous]
    public async Task<IActionResult> GetChannelTypes()
    {
        var channelTypes = await _db.ChannelTypes
            .OrderBy(ct => ct.Name)
            .Select(ct => new { id = ct.Id, name = ct.Name })
            .ToListAsync();
        return Json(channelTypes);
    }

    [HttpPost("api/channels")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateChannelApi([FromBody] CreateChannelRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Tên đại lý không được trống" });

        if (request.ChannelTypeId <= 0)
            return BadRequest(new { error = "Loại đại lý không hợp lệ" });

        var channelType = await _db.ChannelTypes.FindAsync(request.ChannelTypeId);
        if (channelType == null)
            return BadRequest(new { error = "Loại đại lý không tồn tại" });

        var channel = new Channel
        {
            Name = request.Name.Trim(),
            ChannelTypeId = request.ChannelTypeId
        };

        _db.Channels.Add(channel);
        await _db.SaveChangesAsync();

        return Ok(new { id = channel.Id, name = channel.Name });
    }

    public class CreateChannelRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int ChannelTypeId { get; set; }
    }
}
