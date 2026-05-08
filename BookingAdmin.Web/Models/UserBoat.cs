using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAdmin.Web.Models;

public class UserBoat
{
    public int Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public int BoatId { get; set; }
    [ForeignKey(nameof(BoatId))]
    public Boat? Boat { get; set; }
}
