using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public class Currency
{
    public int Id { get; set; }

    [MaxLength(3)]
    [Required]
    public string Code { get; set; } = string.Empty;

    [MaxLength(50)]
    [Required]
    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
