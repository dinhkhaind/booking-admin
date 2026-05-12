using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public class RoomType
{
    public int Id { get; set; }

    [MaxLength(100)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int BaseCapacity { get; set; } = 2;

    public bool IsActive { get; set; } = true;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
