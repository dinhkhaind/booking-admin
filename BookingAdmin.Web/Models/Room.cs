using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAdmin.Web.Models;

public class Room
{
    public int Id { get; set; }

    public int BoatId { get; set; }
    [ForeignKey(nameof(BoatId))]
    public Boat? Boat { get; set; }

    [MaxLength(50)]
    [Required]
    public string RoomCode { get; set; } = string.Empty;

    [MaxLength(200)]
    [Required]
    public string RoomName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Location { get; set; }

    public int TotalRooms { get; set; } = 1;

    public int Capacity { get; set; } = 2;

    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
}
