using Microsoft.AspNetCore.Identity;

namespace SAV.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Foreign Keys
    public int? RoleId { get; set; }

    // Navigation properties
    public Role? Role { get; set; }
    public Client? Client { get; set; }
}
