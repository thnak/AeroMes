using Microsoft.AspNetCore.Authorization;

namespace AeroMes.Api.Auth;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute(string permissionCode)
    : AuthorizeAttribute($"permission:{permissionCode}");
