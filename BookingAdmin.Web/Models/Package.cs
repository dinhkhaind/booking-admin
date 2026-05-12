using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public class Package
{
    public int Id { get; set; }

    [MaxLength(50)]
    [Required]
    public string Code { get; set; } = string.Empty;

    [MaxLength(200)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int AddedDate { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
