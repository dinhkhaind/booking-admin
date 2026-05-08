using System.ComponentModel.DataAnnotations;

namespace BookingAdmin.Web.Models;

public class UserEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    public string Username { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public int[]? SelectedBoats { get; set; }
}
