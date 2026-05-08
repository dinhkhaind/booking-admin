using BookingAdmin.Web.Data;
using BookingAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Controllers;

public class BaseController : Controller
{
    protected readonly AppDbContext _db;

    public BaseController(AppDbContext db) => _db = db;

    protected async Task<List<int>> GetAllowedBoatIds(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return new List<int>();

        if (user.Role == UserRole.Admin)
        {
            return await _db.Boats.Where(b => b.IsActive).Select(b => b.Id).ToListAsync();
        }

        return await _db.UserBoats.Where(ub => ub.UserId == userId).Select(ub => ub.BoatId).ToListAsync();
    }

    protected int? GetSelectedBoatId()
    {
        return HttpContext.Session.GetInt32("SelectedBoatId");
    }

    protected async Task<int?> GetSelectedBoatIdForUser(int userId)
    {
        var boatId = GetSelectedBoatId();
        if (boatId == null) return null;

        var allowedBoatIds = await GetAllowedBoatIds(userId);
        return allowedBoatIds.Contains(boatId.Value) ? boatId : null;
    }
}
