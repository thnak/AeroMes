namespace AeroMes.Domain.Auth;

public class Permission
{
    public int PermissionId { get; private set; }
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string PermissionCode { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private Permission() { }

    public static Permission Create(string resource, string action, string? description = null)
        => new()
        {
            Resource = resource,
            Action = action,
            PermissionCode = $"{resource}:{action}",
            Description = description,
        };
}
