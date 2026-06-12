using AeroMes.Application.Master.Molds.Queries.GetMolds;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Molds.Queries.GetMoldByCode;

public class GetMoldByCodeHandler(IMoldRepository repo)
    : IQueryHandler<GetMoldByCodeQuery, MoldDetailDto?>
{
    public async Task<MoldDetailDto?> HandleAsync(GetMoldByCodeQuery query, CancellationToken ct)
    {
        var m = await repo.GetByCodeWithDetailsAsync(query.Code, ct);
        if (m is null) return null;

        var mappings = m.ProductMappings
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.ProductCode)
            .Select(x => new MoldProductMappingDto(
                x.MappingId, x.ProductCode, x.Product?.ProductName,
                x.IsDefault, x.CycleTimeSeconds))
            .ToList();

        var history = m.MaintenanceLogs
            .OrderByDescending(x => x.StartDate)
            .Select(x => new MoldMaintenanceLogDto(
                x.LogId, x.MaintenanceType.ToString(), x.ShotsAtEvent,
                x.StartDate, x.EndDate, x.TechnicianId, x.Description,
                x.PartReplaced, x.Cost, x.NextDueShots))
            .ToList();

        return new MoldDetailDto(
            m.MoldId, m.MoldCode, m.MoldName,
            m.MoldType.ToString(), m.Material, m.Cavities,
            m.MaxShots, m.CurrentShots, m.ShotsAtLastPm, m.PmIntervalShots,
            m.CurrentShots - m.ShotsAtLastPm,
            GetMoldsHandler.UtilizationPercent(m.CurrentShots, m.MaxShots),
            m.IsPmDue, m.IsNearingEndOfLife,
            m.Status.ToString(),
            m.CurrentMachineCode, m.CurrentMachine?.MachineName,
            m.StorageLocation, m.Manufacturer,
            m.PurchaseDate, m.PurchaseCost, m.WeightKg, m.Notes,
            m.IsActive, mappings, history);
    }
}
