using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public class ChannelType
{
    public int Id { get; set; }

    [MaxLength(50)]
    [Required]
    public string Name { get; set; } = string.Empty;

    public ICollection<Channel> Channels { get; set; } = new List<Channel>();
}
