using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAdmin.Web.Models;

public class Room
{
    public int Id { get; set; }

    [MaxLength(50)]
    [Required]
    public string RoomCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? RoomName { get; set; }

    public int RoomTypeId { get; set; }
    [ForeignKey(nameof(RoomTypeId))]
    public RoomType? RoomType { get; set; }

    public int? TotalRooms { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public int BoatId { get; set; }
    [ForeignKey(nameof(BoatId))]
    public Boat? Boat { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
}
