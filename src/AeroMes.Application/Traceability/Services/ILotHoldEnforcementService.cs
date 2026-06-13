using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Traceability.Services;

/// <summary>
/// Hard-blocks consumption of lots under active hold.
/// Throws <see cref="LotUnderActiveHoldException"/> if a hold exists.
/// </summary>
public interface ILotHoldEnforcementService
{
    Task EnforceNoActiveHoldAsync(string lotNumber, CancellationToken ct = default);
    Task EnforceNoActiveHoldsAsync(IEnumerable<string> lotNumbers, CancellationToken ct = default);
}
