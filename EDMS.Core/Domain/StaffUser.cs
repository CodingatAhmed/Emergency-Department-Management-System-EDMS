namespace EDMS.Core.Domain;

public class StaffUser
{
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public StaffRole Role { get; set; } = StaffRole.Staff;
    public string? DepartmentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
