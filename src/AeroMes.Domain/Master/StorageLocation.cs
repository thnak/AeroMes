using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public enum LocationType { RawMaterial, Wip, FinishedGoods, Scrap }

public class StorageLocation : Entity
{
    public int LocationID { get; private set; }
    public string LocationCode { get; private set; } = string.Empty;
    public string LocationName { get; private set; } = string.Empty;
    public LocationType LocationType { get; private set; }
    public int? WorkCenterID { get; private set; }    // set when type is WIP
    public bool IsActive { get; private set; } = true;

    // EF navigation
    public WorkCenter? WorkCenter { get; private set; }

    private StorageLocation() { }

    public static StorageLocation Create(
        string code,
        string name,
        LocationType type,
        int? workCenterId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Location code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Location name is required.");
        if (type == LocationType.Wip && workCenterId is null)
            throw new DomainException("WIP location must reference a WorkCenter.");

        return new StorageLocation
        {
            LocationCode = code.Trim().ToUpperInvariant(),
            LocationName = name.Trim(),
            LocationType = type,
            WorkCenterID = type == LocationType.Wip ? workCenterId : null,
            IsActive = true,
        };
    }

    public void UpdateDetails(string name, LocationType type, int? workCenterId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Location name is required.");
        if (type == LocationType.Wip && workCenterId is null)
            throw new DomainException("WIP location must reference a WorkCenter.");
        LocationName = name.Trim();
        LocationType = type;
        WorkCenterID = type == LocationType.Wip ? workCenterId : null;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
