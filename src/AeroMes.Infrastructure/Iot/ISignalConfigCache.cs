using AeroMes.Domain.Iot;

namespace AeroMes.Infrastructure.Iot;

public interface ISignalConfigCache
{
    Task<SignalMapping?> ResolveAsync(int adapterId, string sourceAddress, CancellationToken ct = default);
    void Invalidate();
}
