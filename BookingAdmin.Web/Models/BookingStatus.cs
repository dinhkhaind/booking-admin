using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public class BookingStatus
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = "";

    [MaxLength(7)]
    public string Color { get; set; } = "#6c757d";

    public int SortOrder { get; set; }
}
