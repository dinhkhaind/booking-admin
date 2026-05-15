using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAdmin.Web.Models;

public class Booking
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string? SystemCode { get; set; }

    [MaxLength(100)]
    public string? AgencyBookingCode { get; set; }

    [MaxLength(100)]
    public string? BookingCode { get; set; }

    public DateTime? EntryDate { get; set; }

    public int BoatId { get; set; }
    [ForeignKey(nameof(BoatId))]
    public Boat? Boat { get; set; }

    public int ChannelId { get; set; }
    [ForeignKey(nameof(ChannelId))]
    public Channel? Channel { get; set; }

    public int? EmployeeId { get; set; }
    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [MaxLength(200)]
    public string? CustomerName { get; set; }

    public int AdultCount { get; set; }

    public int ChildCount { get; set; }

    public int InfantCount { get; set; }

    public DateOnly CheckIn { get; set; }

    public DateOnly CheckOut { get; set; }

    public int PackageId { get; set; }
    [ForeignKey(nameof(PackageId))]
    public Package? Package { get; set; }

    public decimal TotalPrice { get; set; }

    [NotMapped]
    public decimal Price => TotalPrice;

    public int CurrencyId { get; set; }
    [ForeignKey(nameof(CurrencyId))]
    public Currency? Currency { get; set; }

    public bool HasTransferService { get; set; }

    [MaxLength(200)]
    public string? PickupPoint { get; set; }

    [MaxLength(200)]
    public string? DropoffPoint { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public int StatusId { get; set; }
    [ForeignKey(nameof(StatusId))]
    public BookingStatus? BookingStatus { get; set; }

    public int? EnteredByUserId { get; set; }
    [ForeignKey(nameof(EnteredByUserId))]
    public User? EnteredByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? BoatIdOld { get; set; }

    public int? RoomIdOld { get; set; }

    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
}
