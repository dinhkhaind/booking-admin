using BookingAdmin.Web.Models;
using BCrypt.Net;

namespace BookingAdmin.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Users.Any()) return; // Already seeded

        // Seed Boats
        var hermes = new Boat
        {
            Name = "Hermes",
            Description = "Luxury cruise ship",
            IsActive = true
        };
        db.Boats.Add(hermes);

        // Seed Currencies
        var vnd = new Currency { Code = "VND", Name = "Vietnamese Dong" };
        var usd = new Currency { Code = "USD", Name = "US Dollar" };
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Khai@1234"),
            Role = UserRole.Admin,
            IsActive = true
        };
        db.Users.Add(admin);

        await db.SaveChangesAsync();
    }
}
