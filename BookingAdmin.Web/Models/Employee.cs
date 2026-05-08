using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public class Employee
{
    public int Id { get; set; }

    [MaxLength(100)]
    [Required]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
