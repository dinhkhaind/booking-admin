using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAdmin.Web.Models;

public class BookingRoom
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }

    public int RoomId { get; set; }
    [ForeignKey(nameof(RoomId))]
    public Room? Room { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;
}
