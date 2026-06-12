using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.OrgUnits.Commands.SyncOrgUnits;

public class SyncOrgUnitsHandler(IOrgUnitRepository repo, IUnitOfWork uow)
    : ICommandHandler<SyncOrgUnitsCommand, SyncOrgUnitsResult>
{
    public async Task<SyncOrgUnitsResult> HandleAsync(SyncOrgUnitsCommand cmd, CancellationToken ct)
    {
        var existing = await repo.GetAllForSyncAsync(ct);
        var byCode = existing.ToDictionary(x => x.UnitCode, StringComparer.OrdinalIgnoreCase);

        int created = 0, updated = 0, deactivated = 0;

        // Pass 1 — upsert every unit in the snapshot.
        foreach (var entry in cmd.Units)
        {
            var code = entry.UnitCode.Trim().ToUpperInvariant();
            if (byCode.TryGetValue(code, out var unit))
            {
                unit.ApplySync(entry.UnitName, entry.UnitType, entry.IsActive, entry.SourceSystemId, cmd.SyncedBy);
                updated++;
            }
            else
            {
                unit = OrgUnit.Create(code, entry.UnitName, entry.UnitType, entry.SourceSystemId, cmd.SyncedBy);
                if (!entry.IsActive)
                    unit.DeactivateFromSync(cmd.SyncedBy);
                await repo.AddAsync(unit, ct);
                byCode[code] = unit;
                created++;
            }
        }

        // Persist inserts so new units get their UnitId before parent linking.
        await uow.SaveChangesAsync(ct);

        // Pass 2 — resolve parent references by code (parents may appear in any order).
        foreach (var entry in cmd.Units)
        {
            var unit = byCode[entry.UnitCode.Trim().ToUpperInvariant()];
            if (string.IsNullOrWhiteSpace(entry.ParentUnitCode))
            {
                unit.SetParent(null);
                continue;
            }

            var parentCode = entry.ParentUnitCode.Trim().ToUpperInvariant();
            if (!byCode.TryGetValue(parentCode, out var parent))
                throw new DomainException(
                    $"Đơn vị cha '{parentCode}' của '{unit.UnitCode}' không tồn tại trong dữ liệu đồng bộ.");
            unit.SetParent(parent.UnitId);
        }

        // Pass 3 — units that vanished from the AMIS snapshot are deactivated, never deleted.
        var snapshotCodes = cmd.Units
            .Select(u => u.UnitCode.Trim().ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var unit in existing.Where(x => x.IsActive && !snapshotCodes.Contains(x.UnitCode)))
        {
            unit.DeactivateFromSync(cmd.SyncedBy);
            deactivated++;
        }

        await uow.SaveChangesAsync(ct);
        return new SyncOrgUnitsResult(created, updated, deactivated);
    }
}
