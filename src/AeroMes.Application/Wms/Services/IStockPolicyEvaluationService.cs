namespace AeroMes.Application.Wms.Services;

public interface IStockPolicyEvaluationService
{
    Task EvaluateAsync(string productCode, int locationId, CancellationToken ct);
}
