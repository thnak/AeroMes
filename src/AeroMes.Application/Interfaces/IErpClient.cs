namespace AeroMes.Application.Interfaces;

public interface IErpClient
{
    Task<IReadOnlyList<ErpSalesOrderRecord>> GetSalesOrdersAsync(DateTime? since, CancellationToken ct = default);
    Task<IReadOnlyList<ErpProductionOrderRecord>> GetProductionOrdersAsync(DateTime? since, CancellationToken ct = default);
    Task<bool> TestConnectionAsync(CancellationToken ct = default);
}

public record ErpSalesOrderRecord(
    string SOCode,
    DateTime OrderDate,
    string? CustomerName,
    DateTime? DeliveryDate,
    string? CustomerCode);

public record ErpProductionOrderRecord(
    string POCode,
    string ProductCode,
    int TargetQuantity,
    string? SOCode,
    DateTime? PlannedStart,
    DateTime? PlannedEnd);
