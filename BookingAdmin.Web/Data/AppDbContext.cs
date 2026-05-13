using BookingAdmin.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAdmin.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Boat> Boats => Set<Boat>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<ChannelType> ChannelTypes => Set<ChannelType>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<BookingStatus> BookingStatuses => Set<BookingStatus>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
    public DbSet<SaleEntry> SaleEntries => Set<SaleEntry>();
    public DbSet<UserBoat> UserBoats => Set<UserBoat>();
    public DbSet<RoomBlock> RoomBlocks => Set<RoomBlock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Boat>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<RoomType>(rt =>
        {
            rt.HasKey(x => x.Id);
            rt.HasIndex(x => x.Name).IsUnique();
            rt.HasMany(x => x.Rooms)
                .WithOne(r => r.RoomType)
                .HasForeignKey(r => r.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Room>(r =>
        {
            r.HasKey(x => x.Id);
            r.HasIndex(x => new { x.BoatId, x.RoomCode }).IsUnique();
            r.HasOne(x => x.Boat)
                .WithMany(b => b.Rooms)
                .HasForeignKey(x => x.BoatId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Package>(p =>
        {
            p.HasKey(x => x.Id);
            p.HasIndex(x => x.Code).IsUnique();
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
            c.HasOne(x => x.ChannelType)
                .WithMany(ct => ct.Channels)
                .HasForeignKey(x => x.ChannelTypeId)
                .OnDelete(DeleteBehavior.Restrict);
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
            c.Property(x => x.IsDefault).HasDefaultValue(false);
        });

        modelBuilder.Entity<BookingStatus>(bs =>
        {
            bs.HasKey(x => x.Id);
            bs.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Booking>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.SystemCode);
            b.HasIndex(x => new { x.BoatId, x.CheckIn });
            b.HasIndex(x => x.AgencyBookingCode);
            b.Property(x => x.TotalPrice).HasPrecision(18, 2);
            b.Property(x => x.CheckIn).HasColumnType("date");
            b.Property(x => x.CheckOut).HasColumnType("date");
            b.HasOne(x => x.Boat)
                .WithMany(bo => bo.Bookings)
                .HasForeignKey(x => x.BoatId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Channel)
                .WithMany(c => c.Bookings)
                .HasForeignKey(x => x.ChannelId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Employee)
                .WithMany(e => e.Bookings)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(x => x.Currency)
                .WithMany(c => c.Bookings)
                .HasForeignKey(x => x.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Package)
                .WithMany(p => p.Bookings)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.BookingStatus)
                .WithMany()
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingRoom>(br =>
        {
            br.HasKey(x => x.Id);
            br.HasIndex(x => new { x.BookingId, x.RoomId }).IsUnique();
            br.HasOne(x => x.Booking)
                .WithMany(b => b.BookingRooms)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            br.HasOne(x => x.Room)
                .WithMany(r => r.BookingRooms)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserBoat>(ub =>
        {
            ub.HasKey(x => x.Id);
            ub.HasIndex(x => new { x.UserId, x.BoatId }).IsUnique();
            ub.HasOne(x => x.User)
                .WithMany(u => u.UserBoats)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            ub.HasOne(x => x.Boat)
                .WithMany(b => b.UserBoats)
                .HasForeignKey(x => x.BoatId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomBlock>(rb =>
        {
            rb.HasKey(x => x.Id);
            rb.HasIndex(x => new { x.RoomId, x.FromDate, x.ToDate });
            rb.HasOne(x => x.Boat)
                .WithMany()
                .HasForeignKey(x => x.BoatId)
                .OnDelete(DeleteBehavior.Restrict);
            rb.HasOne(x => x.Room)
                .WithMany()
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
            rb.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
