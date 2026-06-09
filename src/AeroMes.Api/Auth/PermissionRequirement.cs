using Microsoft.AspNetCore.Authorization;

namespace AeroMes.Api.Auth;

public record PermissionRequirement(string PermissionCode) : IAuthorizationRequirement;
