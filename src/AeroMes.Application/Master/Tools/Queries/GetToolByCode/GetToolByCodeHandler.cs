using AeroMes.Application.Master.Tools.Queries.GetTools;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Tools.Queries.GetToolByCode;

public class GetToolByCodeHandler(IToolRepository repo)
    : IQueryHandler<GetToolByCodeQuery, ToolDetailDto?>
{
    /// <summary>Checkout history is capped to the most recent entries to keep the payload bounded.</summary>
    private const int MaxCheckoutHistory = 20;

    public async Task<ToolDetailDto?> HandleAsync(GetToolByCodeQuery query, CancellationToken ct)
    {
        var t = await repo.GetByCodeWithDetailsAsync(query.Code, ct);
        if (t is null) return null;

        var mappings = t.OperationMappings
            .OrderBy(x => x.OperationCode)
            .ThenBy(x => x.ProductCode)
            .Select(x => new ToolOperationMappingDto(
                x.MappingId, x.OperationCode, x.Operation?.OperationName,
                x.ProductCode, x.Product?.ProductName,
                x.IsRequired, x.UsageCountPerOp))
            .ToList();

        var checkouts = t.Checkouts
            .OrderByDescending(x => x.CheckedOutAt)
            .Take(MaxCheckoutHistory)
            .Select(x => new ToolCheckoutDto(
                x.CheckoutId, x.WorkCenterId, x.WorkCenter?.WorkCenterName,
                x.CheckedOutBy, x.CheckedOutAt, x.ExpectedReturnAt,
                x.ReturnedAt, x.ReturnedBy,
                x.ConditionOnReturn?.ToString(), x.Notes))
            .ToList();

        var maintenance = t.MaintenanceLogs
            .OrderByDescending(x => x.PerformedAt)
            .Select(x => new ToolMaintenanceLogDto(
                x.LogId, x.MaintenanceType.ToString(), x.UsageAtEvent,
                x.PerformedAt, x.PerformedBy, x.Cost,
                x.NextDueCount, x.NextDueDate, x.Notes))
            .ToList();

        return new ToolDetailDto(
            t.ToolId, t.ToolCode, t.ToolName,
            t.ToolType.ToString(), t.Brand, t.Model, t.Specification,
            t.MaxUsageCount, t.CurrentUsageCount, t.UsageCountAtLastPm, t.PmIntervalCount,
            t.CurrentUsageCount - t.UsageCountAtLastPm,
            GetToolsHandler.UsagePercent(t.CurrentUsageCount, t.MaxUsageCount),
            t.IsReconditioningDue, t.IsNearingEndOfLife,
            t.RequiresCalibration, t.CalibrationIntervalDays,
            t.LastCalibratedAt, t.NextCalibrationDue,
            t.Status.ToString(),
            t.CurrentWorkCenterId, t.CurrentWorkCenter?.WorkCenterName,
            t.StorageLocation, t.PurchaseDate, t.PurchaseCost, t.Notes,
            t.IsActive, mappings, checkouts, maintenance);
    }
}
