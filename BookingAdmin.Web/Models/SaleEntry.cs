using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public class SaleEntry
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string? BoatName { get; set; }

    [MaxLength(200)]
    public string? BookingCode { get; set; }

    [MaxLength(200)]
    public string? Agency { get; set; }

    [MaxLength(50)]
    public string? Source { get; set; }

    [MaxLength(100)]
    public string? Sale { get; set; }

    public decimal? Price { get; set; }

    public DateOnly? CheckInDate { get; set; }

    [MaxLength(20)]
    public string? Itinerary { get; set; }

    public int? RoomQty { get; set; }

    public DateOnly? TrackingDate { get; set; }

    [MaxLength(50)]
    public string? CompanyCar { get; set; }

    public int? GuestQty { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }

    public DateTime ImportedAt { get; set; }
}
