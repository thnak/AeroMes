namespace AeroMes.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsAsync(string userId, CancellationToken ct = default);
}
