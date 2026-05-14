using BookingAdmin.Web.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Seed colors for existing RoomTypes that don't have colors
        var colorPalette = new[] { "#2563eb", "#059669", "#d97706", "#dc2626", "#7c3aed", "#0891b2", "#be185d", "#65a30d", "#ea580c", "#0d9488" };
        var roomTypesWithoutColor = await db.RoomTypes.Where(rt => rt.Color == null).OrderBy(rt => rt.Id).ToListAsync();
        var colorIndex = 0;
        foreach (var rt in roomTypesWithoutColor)
        {
            rt.Color = colorPalette[colorIndex % colorPalette.Length];
            colorIndex++;
        }
        if (roomTypesWithoutColor.Any())
        {
            db.RoomTypes.UpdateRange(roomTypesWithoutColor);
            await db.SaveChangesAsync();
        }

        if (db.Users.Any()) return; // Already seeded

        // Seed BookingStatuses (must be first for FK)
        if (!db.BookingStatuses.Any())
        {
            db.BookingStatuses.AddRange(
                new BookingStatus { Id = 1, Code = "Pending", Name = "Pending", Color = "#ffc107", SortOrder = 1 },
                new BookingStatus { Id = 2, Code = "Confirmed", Name = "Confirmed", Color = "#28a745", SortOrder = 2 },
                new BookingStatus { Id = 3, Code = "Cancelled", Name = "Cancelled", Color = "#dc3545", SortOrder = 3 },
                new BookingStatus { Id = 4, Code = "Block", Name = "Block", Color = "#6c757d", SortOrder = 4 }
            );
        }

        // Seed Packages
        if (!db.Packages.Any())
        {
            db.Packages.AddRange(
                new Package { Code = "2N1D", Name = "2 Nights 1 Day", Description = "2 nights 1 day package", AddedDate = 1, IsActive = true },
                new Package { Code = "3N2D", Name = "3 Nights 2 Days", Description = "3 nights 2 days package", AddedDate = 2, IsActive = true }
            );
        }

        // Seed Boats
        var hermes = new Boat
        {
            Name = "Hermes",
            Description = "Luxury cruise ship",
            IsActive = true
        };
        db.Boats.Add(hermes);

        // Seed Currencies
        var vnd = new Currency { Code = "VND", Name = "Vietnamese Dong", IsDefault = true };
        var usd = new Currency { Code = "USD", Name = "US Dollar", IsDefault = false };
        db.Currencies.AddRange(vnd, usd);

        // Seed ChannelType
        var onlineChannelType = new ChannelType { Name = "Online" };
        db.ChannelTypes.Add(onlineChannelType);

        await db.SaveChangesAsync();

        // Seed Channels (after ChannelType is saved)
        var bookingCom = new Channel
        {
            ChannelTypeId = onlineChannelType.Id,
            Name = "Booking.com"
        };
        var expedia = new Channel
        {
            ChannelTypeId = onlineChannelType.Id,
            Name = "Expedia"
        };
        var agoda = new Channel
        {
            ChannelTypeId = onlineChannelType.Id,
            Name = "Agoda"
        };
        db.Channels.AddRange(bookingCom, expedia, agoda);

        // Seed Admin User
        var admin = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Khaivd@1234"),
            Role = UserRole.Admin,
            IsActive = true
        };
        db.Users.Add(admin);

        await db.SaveChangesAsync();
    }
}
