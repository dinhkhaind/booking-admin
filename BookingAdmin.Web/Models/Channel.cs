using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAdmin.Web.Models;

public class Channel
{
    public int Id { get; set; }

    public int ChannelTypeId { get; set; }
    [ForeignKey(nameof(ChannelTypeId))]
    public ChannelType? ChannelType { get; set; }

    [MaxLength(100)]
    [Required]
    public string Name { get; set; } = string.Empty;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
