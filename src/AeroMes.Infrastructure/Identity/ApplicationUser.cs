using Microsoft.AspNetCore.Identity;

namespace AeroMes.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public bool ForcePasswordChange { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? AvatarUrl { get; set; }
    public int? DefaultWorkCenterId { get; set; }
}
