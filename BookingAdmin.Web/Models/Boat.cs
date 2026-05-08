using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public class Boat
{
    public int Id { get; set; }

    [MaxLength(100)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<UserBoat> UserBoats { get; set; } = new List<UserBoat>();
}
