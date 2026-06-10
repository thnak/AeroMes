using AeroMes.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AeroMes.Infrastructure.Identity;

public class UserRepository(UserManager<ApplicationUser> userManager) : IUserRepository
{
    public async Task<bool> ExistsAsync(string userId, CancellationToken ct = default)
        => await userManager.FindByIdAsync(userId) is not null;
}
