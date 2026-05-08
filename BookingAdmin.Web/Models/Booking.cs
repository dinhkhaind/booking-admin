using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAdmin.Web.Models;

public class Booking
{
    public int Id { get; set; }

    public int BoatId { get; set; }
    [ForeignKey(nameof(BoatId))]
    public Boat? Boat { get; set; }

    public int ChannelId { get; set; }
    [ForeignKey(nameof(ChannelId))]
    public Channel? Channel { get; set; }

    public int? EmployeeId { get; set; }
    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [MaxLength(100)]
    public string? BookingCode { get; set; }

    [MaxLength(200)]
    public string? CustomerName { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateOnly CheckIn { get; set; }

    public DateOnly CheckOut { get; set; }

    public DateOnly EntryDate { get; set; }

    public decimal Price { get; set; }

    public int CurrencyId { get; set; }
    [ForeignKey(nameof(CurrencyId))]
    public Currency? Currency { get; set; }

    public bool TransferUsed { get; set; }

    [MaxLength(200)]
    public string? PickupPoint { get; set; }

    [MaxLength(200)]
    public string? DropoffPoint { get; set; }

    public int AdultCount { get; set; }

    public int ChildCount { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
}
