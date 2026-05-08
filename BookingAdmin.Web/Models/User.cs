using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public enum UserRole
{
    Admin,
    Manager,
    BookingStaff,
    Viewer
}

public class User
{
    public int Id { get; set; }

    [MaxLength(100)]
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Viewer;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserBoat> UserBoats { get; set; } = new List<UserBoat>();
}
