using AeroMes.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AeroMes.Infrastructure.Identity;

public class RoleRepository(RoleManager<IdentityRole> roleManager) : IRoleRepository
{
    public async Task<bool> ExistsAsync(string roleId, CancellationToken ct = default)
        => await roleManager.FindByIdAsync(roleId) is not null;
}
