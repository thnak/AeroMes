namespace AeroMes.Domain.Auth;

public class RolePermission
{
    public string RoleId { get; private set; } = string.Empty;
    public int PermissionId { get; private set; }

    private RolePermission() { }

    public static RolePermission Create(string roleId, int permissionId)
        => new() { RoleId = roleId, PermissionId = permissionId };
}
