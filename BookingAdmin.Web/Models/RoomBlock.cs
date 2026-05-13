using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAdmin.Web.Models;

public class RoomBlock
{
    public int Id { get; set; }

    public int BoatId { get; set; }
    [ForeignKey(nameof(BoatId))]
    public Boat? Boat { get; set; }

    public int RoomId { get; set; }
    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }

    [Required]
    public DateOnly FromDate { get; set; }

    [Required]
    public DateOnly ToDate { get; set; }

    [MaxLength(200)]
    public string? Partner { get; set; }

    public DateTime? Deadline { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }
    [ForeignKey(nameof(CreatedByUserId))]
    public User? CreatedByUser { get; set; }

    public bool IsActive { get; set; } = true;
}
