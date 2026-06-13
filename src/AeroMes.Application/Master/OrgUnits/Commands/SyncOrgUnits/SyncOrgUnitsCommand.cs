using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.OrgUnits.Commands.SyncOrgUnits;

/// <summary>
/// Full-snapshot synchronization from the AMIS System (source of truth).
/// Units present in the snapshot are upserted; units missing from it are deactivated.
/// </summary>
public record SyncOrgUnitsCommand(
    IReadOnlyList<OrgUnitSyncEntry> Units,
    string? SyncedBy) : ICommand<ValidationResult<SyncOrgUnitsResult>>;

public record OrgUnitSyncEntry(
    string UnitCode,
    string UnitName,
    string? ParentUnitCode,
    OrgUnitType UnitType,
    bool IsActive,
    string SourceSystemId);

public record SyncOrgUnitsResult(int Created, int Updated, int Deactivated);
