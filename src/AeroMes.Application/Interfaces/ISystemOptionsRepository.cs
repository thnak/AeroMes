using AeroMes.Domain.Settings;

namespace AeroMes.Application.Interfaces;

public interface ISystemOptionsRepository
{
    Task<SystemOptions> GetAsync(CancellationToken ct);
}
