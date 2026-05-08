using BookingAdmin.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Boat> Boats => Set<Boat>();
    public DbSet<ChannelType> ChannelTypes => Set<ChannelType>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
    public DbSet<SaleEntry> SaleEntries => Set<SaleEntry>();
    public DbSet<UserBoat> UserBoats => Set<UserBoat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Boat>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<ChannelType>(ct =>
        {
            ct.HasKey(x => x.Id);
            ct.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Channel>(c =>
        {
            c.HasKey(x => x.Id);
            c.HasIndex(x => x.Name);
            c.HasOne(x => x.ChannelType).WithMany(ct => ct.Channels).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.FullName);
        });

        modelBuilder.Entity<Currency>(c =>
        {
            c.HasKey(x => x.Id);
            c.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Room>(r =>
        {
            r.HasKey(x => x.Id);
            r.HasIndex(x => new { x.BoatId, x.RoomCode });
            r.HasOne(x => x.Boat).WithMany(b => b.Rooms).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.BoatId, x.CheckIn });
            b.HasIndex(x => x.BookingCode);
            b.Property(x => x.Price).HasPrecision(18, 2);
            b.Property(x => x.CheckIn).HasColumnType("date");
            b.Property(x => x.CheckOut).HasColumnType("date");
            b.Property(x => x.EntryDate).HasColumnType("date");
            b.HasOne(x => x.Boat).WithMany(bo => bo.Bookings).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Channel).WithMany(c => c.Bookings).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Employee).WithMany(e => e.Bookings).OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.Currency).WithMany(c => c.Bookings).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingRoom>(br =>
        {
            br.HasKey(x => x.Id);
            br.HasIndex(x => new { x.BookingId, x.RoomId }).IsUnique();
            br.HasOne(x => x.Booking).WithMany(b => b.BookingRooms).OnDelete(DeleteBehavior.Cascade);
            br.HasOne(x => x.Room).WithMany(r => r.BookingRooms).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserBoat>(ub =>
        {
            ub.HasKey(x => x.Id);
            ub.HasIndex(x => new { x.UserId, x.BoatId }).IsUnique();
            ub.HasOne(x => x.User).WithMany(u => u.UserBoats).OnDelete(DeleteBehavior.Cascade);
            ub.HasOne(x => x.Boat).WithMany(b => b.UserBoats).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
