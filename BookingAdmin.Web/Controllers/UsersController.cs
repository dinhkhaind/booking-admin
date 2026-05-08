using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

[Authorize(Roles = "Admin")] // Only Admin can manage users
public class UsersController : Controller
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var users = await _db.Users.OrderBy(u => u.Username).ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
        ViewBag.Boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model, int[] selectedBoats)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
            ViewBag.Boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = string.Join(" | ", errors);
            return View(model);
        }

        var user = new User
        {
            Username = model.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = model.Role,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Save boat permissions
        if (selectedBoats != null && selectedBoats.Length > 0)
        {
            var userBoats = selectedBoats.Distinct().Select(boatId => new UserBoat
            {
                UserId = user.Id,
                BoatId = boatId
            }).ToList();
            _db.UserBoats.AddRange(userBoats);
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = $"User '{user.Username}' created successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await _db.Users.Include(u => u.UserBoats).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        var model = new UserEditViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            SelectedBoats = user.UserBoats.Select(ub => ub.BoatId).ToArray()
        };

        ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
        ViewBag.Boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
            ViewBag.Boats = await _db.Boats.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["Error"] = string.Join(" | ", errors);
            return View(model);
        }

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Username = model.Username;
        user.Role = model.Role;
        user.IsActive = model.IsActive;

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        // Update boat permissions
        var existingUserBoats = await _db.UserBoats.Where(ub => ub.UserId == id).ToListAsync();
        _db.UserBoats.RemoveRange(existingUserBoats);

        if (model.SelectedBoats != null && model.SelectedBoats.Length > 0)
        {
            var newUserBoats = model.SelectedBoats.Distinct().Select(boatId => new UserBoat
            {
                UserId = id,
                BoatId = boatId
            }).ToList();
            _db.UserBoats.AddRange(newUserBoats);
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"User '{user.Username}' updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, User user)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();
        var username = u.Username;
        _db.Users.Remove(u);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"User '{username}' deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
