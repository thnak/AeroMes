using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Events;

namespace AeroMes.Domain.Master;

public enum MoldStatus { Active, InMaintenance, InRepair, Standby, Scrapped }
public enum MoldType { Injection, DieCast, Stamping, Blow, Forging, Other }
public enum MoldMaintenanceType { Pm, Repair, Inspection, Overhaul }

public class Mold : AuditableEntity
{
    /// <summary>Shot fraction of MaxShots at which the end-of-life warning fires.</summary>
    public const double EndOfLifeWarningRatio = 0.95;

    public int MoldId { get; private set; }
    public string MoldCode { get; private set; } = string.Empty;
    public string MoldName { get; private set; } = string.Empty;
    public MoldType MoldType { get; private set; }
    public string? Material { get; private set; }          // P20, H13, S45C...
    public int Cavities { get; private set; } = 1;
    public long MaxShots { get; private set; }             // manufacturer's rated life
    public long CurrentShots { get; private set; }
    public long ShotsAtLastPm { get; private set; }
    public int PmIntervalShots { get; private set; }
    public string? Manufacturer { get; private set; }
    public DateOnly? PurchaseDate { get; private set; }
    public decimal? PurchaseCost { get; private set; }
    public decimal? WeightKg { get; private set; }
    public MoldStatus Status { get; private set; } = MoldStatus.Active;
    public string? CurrentMachineCode { get; private set; } // where mounted; null = in storage
    public string? StorageLocation { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF navigations
    public Machine? CurrentMachine { get; private set; }

    private readonly List<MoldProductMapping> _productMappings = [];
    public IReadOnlyList<MoldProductMapping> ProductMappings => _productMappings.AsReadOnly();

    private readonly List<MoldMaintenanceLog> _maintenanceLogs = [];
    public IReadOnlyList<MoldMaintenanceLog> MaintenanceLogs => _maintenanceLogs.AsReadOnly();

    private Mold() { }

    public static Mold Create(
        string code, string name, MoldType moldType,
        string? material, int cavities, long maxShots, int pmIntervalShots,
        string? manufacturer, DateOnly? purchaseDate, decimal? purchaseCost, decimal? weightKg,
        string? storageLocation, string? notes, string? createdBy)
    {
        ValidateLife(cavities, maxShots, pmIntervalShots);
        return new Mold
        {
            MoldCode = code.Trim().ToUpperInvariant(),
            MoldName = name.Trim(),
            MoldType = moldType,
            Material = material?.Trim(),
            Cavities = cavities,
            MaxShots = maxShots,
            PmIntervalShots = pmIntervalShots,
            Manufacturer = manufacturer?.Trim(),
            PurchaseDate = purchaseDate,
            PurchaseCost = purchaseCost,
            WeightKg = weightKg,
            StorageLocation = storageLocation?.Trim(),
            Notes = notes,
            Status = MoldStatus.Active,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(
        string name, MoldType moldType,
        string? material, int cavities, long maxShots, int pmIntervalShots,
        string? manufacturer, DateOnly? purchaseDate, decimal? purchaseCost, decimal? weightKg,
        string? storageLocation, string? notes, bool isActive, string? updatedBy)
    {
        if (Status == MoldStatus.Scrapped)
            throw new DomainException($"Khuôn '{MoldCode}' đã thanh lý, không thể cập nhật.");
        ValidateLife(cavities, maxShots, pmIntervalShots);

        MoldName = name.Trim();
        MoldType = moldType;
        Material = material?.Trim();
        Cavities = cavities;
        MaxShots = maxShots;
        PmIntervalShots = pmIntervalShots;
        Manufacturer = manufacturer?.Trim();
        PurchaseDate = purchaseDate;
        PurchaseCost = purchaseCost;
        WeightKg = weightKg;
        StorageLocation = storageLocation?.Trim();
        Notes = notes;
        IsActive = isActive;
        Touch(updatedBy);
    }

    public MoldProductMapping AddProductMapping(
        string productCode, bool isDefault, double? cycleTimeSeconds, string? updatedBy)
    {
        var normalized = productCode.Trim().ToUpperInvariant();
        if (_productMappings.Any(x => x.ProductCode == normalized))
            throw new DomainException($"Sản phẩm '{normalized}' đã được khai báo cho khuôn '{MoldCode}'.");

        // Only one mapping can be the mold's primary product.
        if (isDefault)
            foreach (var existing in _productMappings)
                existing.ClearDefault();

        var mapping = MoldProductMapping.Create(MoldId, normalized, isDefault, cycleTimeSeconds);
        _productMappings.Add(mapping);
        Touch(updatedBy);
        return mapping;
    }

    public void RemoveProductMapping(int mappingId, string? updatedBy)
    {
        var mapping = _productMappings.FirstOrDefault(x => x.MappingId == mappingId)
            ?? throw new EntityNotFoundException(nameof(MoldProductMapping), mappingId);
        _productMappings.Remove(mapping);
        Touch(updatedBy);
    }

    public void AssignToMachine(string machineCode, string? updatedBy)
    {
        if (Status != MoldStatus.Active)
            throw new DomainException(
                $"Chỉ khuôn ở trạng thái Active mới được gắn lên máy. Trạng thái hiện tại: {Status}.");
        if (CurrentMachineCode is not null)
            throw new DomainException(
                $"Khuôn '{MoldCode}' đang được gắn trên máy '{CurrentMachineCode}'. Tháo khuôn trước khi gắn sang máy khác.");
        if (_productMappings.Count == 0)
            throw new DomainException(
                $"Khuôn '{MoldCode}' phải có ít nhất một sản phẩm được khai báo trước khi gắn lên máy.");

        CurrentMachineCode = machineCode.Trim().ToUpperInvariant();
        Touch(updatedBy);
        RaiseDomainEvent(new MoldMountedEvent(MoldId, MoldCode, CurrentMachineCode));
    }

    public void Unmount(string? updatedBy)
    {
        if (CurrentMachineCode is null)
            throw new DomainException($"Khuôn '{MoldCode}' không được gắn trên máy nào.");

        var machineCode = CurrentMachineCode;
        CurrentMachineCode = null;
        Touch(updatedBy);
        RaiseDomainEvent(new MoldUnmountedEvent(MoldId, MoldCode, machineCode));
    }

    public void SendForMaintenance(MoldMaintenanceType maintenanceType, string? updatedBy)
    {
        if (Status is not (MoldStatus.Active or MoldStatus.Standby))
            throw new DomainException(
                $"Chỉ khuôn ở trạng thái Active hoặc Standby mới được đưa đi bảo trì. Trạng thái hiện tại: {Status}.");
        if (CurrentMachineCode is not null)
            throw new DomainException($"Phải tháo khuôn '{MoldCode}' khỏi máy trước khi đưa đi bảo trì.");

        Status = maintenanceType == MoldMaintenanceType.Repair
            ? MoldStatus.InRepair
            : MoldStatus.InMaintenance;
        Touch(updatedBy);
    }

    public MoldMaintenanceLog CompleteMaintenance(
        MoldMaintenanceType maintenanceType,
        DateTime startDate, DateTime? endDate,
        string? technicianId, string? description, string? partReplaced,
        decimal? cost, long? nextDueShots, string? updatedBy)
    {
        if (Status is not (MoldStatus.InMaintenance or MoldStatus.InRepair))
            throw new DomainException(
                $"Khuôn '{MoldCode}' không ở trạng thái bảo trì. Trạng thái hiện tại: {Status}.");

        var log = MoldMaintenanceLog.Create(
            MoldId, maintenanceType, CurrentShots,
            startDate, endDate, technicianId, description, partReplaced, cost, nextDueShots);
        _maintenanceLogs.Add(log);

        // PM and overhaul reset the PM cycle; repairs and inspections do not.
        if (maintenanceType is MoldMaintenanceType.Pm or MoldMaintenanceType.Overhaul)
            ShotsAtLastPm = CurrentShots;

        Status = MoldStatus.Active;
        Touch(updatedBy);
        return log;
    }

    public void AccumulateShots(long shots, string? updatedBy)
    {
        if (Status != MoldStatus.Active)
            throw new DomainException(
                $"Chỉ ghi nhận shot cho khuôn đang hoạt động. Trạng thái hiện tại: {Status}.");

        CurrentShots += shots;
        Touch(updatedBy);

        if (IsPmDue)
            RaiseDomainEvent(new MoldPmDueEvent(MoldId, MoldCode, CurrentShots, ShotsAtLastPm, PmIntervalShots));
        if (IsNearingEndOfLife)
            RaiseDomainEvent(new MoldNearingEndOfLifeEvent(MoldId, MoldCode, CurrentShots, MaxShots));
    }

    public void Scrap(string? updatedBy)
    {
        if (Status == MoldStatus.Scrapped)
            throw new DomainException($"Khuôn '{MoldCode}' đã được thanh lý trước đó.");
        if (CurrentMachineCode is not null)
            throw new DomainException($"Phải tháo khuôn '{MoldCode}' khỏi máy trước khi thanh lý.");

        Status = MoldStatus.Scrapped;
        IsActive = false;
        Touch(updatedBy);
    }

    public bool IsPmDue => CurrentShots - ShotsAtLastPm >= PmIntervalShots;
    public bool IsNearingEndOfLife => CurrentShots >= MaxShots * EndOfLifeWarningRatio;

    private static void ValidateLife(int cavities, long maxShots, int pmIntervalShots)
    {
        if (cavities < 1)
            throw new DomainException("Số lòng khuôn (cavities) phải lớn hơn hoặc bằng 1.");
        if (maxShots <= 0)
            throw new DomainException("Số shot tối đa (MaxShots) phải lớn hơn 0.");
        if (pmIntervalShots <= 0)
            throw new DomainException("Chu kỳ bảo trì theo shot (PmIntervalShots) phải lớn hơn 0.");
    }
}
