using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Events;

namespace AeroMes.Domain.Master;

public enum ToolType { CuttingTool, Fixture, Jig, Gauge, Clamp, Die, Electrode, ConsumableTool }
public enum ToolStatus { Available, InUse, InCalibration, InRepair, Scrapped }
public enum ToolMaintenanceType { Reconditioning, Calibration, Repair, Inspection }
public enum ToolReturnCondition { Good, Worn, Damaged, Missing }
public enum ToolServiceType { Calibration, Repair }

public class Tool : AuditableEntity
{
    /// <summary>Usage fraction of MaxUsageCount at which the end-of-life warning fires.</summary>
    public const double EndOfLifeWarningRatio = 0.9;

    public int ToolId { get; private set; }
    public string ToolCode { get; private set; } = string.Empty;
    public string ToolName { get; private set; } = string.Empty;
    public ToolType ToolType { get; private set; }
    public string? Brand { get; private set; }
    public string? Model { get; private set; }
    public string? Specification { get; private set; }      // e.g. 'Ø12 HSS End Mill 4-flute'
    public int? MaxUsageCount { get; private set; }          // null = no life limit (durable fixture)
    public int CurrentUsageCount { get; private set; }
    public int UsageCountAtLastPm { get; private set; }
    public int? PmIntervalCount { get; private set; }        // uses between reconditioning
    public bool RequiresCalibration { get; private set; }
    public int? CalibrationIntervalDays { get; private set; }
    public DateTime? LastCalibratedAt { get; private set; }
    public DateOnly? NextCalibrationDue { get; private set; }
    public string? StorageLocation { get; private set; }
    public int? CurrentWorkCenterId { get; private set; }    // where checked out; null = tool room
    public ToolStatus Status { get; private set; } = ToolStatus.Available;
    public DateOnly? PurchaseDate { get; private set; }
    public decimal? PurchaseCost { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF navigations
    public WorkCenter? CurrentWorkCenter { get; private set; }

    private readonly List<ToolOperationMapping> _operationMappings = [];
    public IReadOnlyList<ToolOperationMapping> OperationMappings => _operationMappings.AsReadOnly();

    private readonly List<ToolCheckout> _checkouts = [];
    public IReadOnlyList<ToolCheckout> Checkouts => _checkouts.AsReadOnly();

    private readonly List<ToolMaintenanceLog> _maintenanceLogs = [];
    public IReadOnlyList<ToolMaintenanceLog> MaintenanceLogs => _maintenanceLogs.AsReadOnly();

    private Tool() { }

    public static Tool Create(
        string code, string name, ToolType toolType,
        string? brand, string? model, string? specification,
        int? maxUsageCount, int? pmIntervalCount,
        bool requiresCalibration, int? calibrationIntervalDays,
        string? storageLocation, DateOnly? purchaseDate, decimal? purchaseCost,
        string? notes, string? createdBy)
    {
        ValidateLife(maxUsageCount, pmIntervalCount, requiresCalibration, calibrationIntervalDays);
        return new Tool
        {
            ToolCode = code.Trim().ToUpperInvariant(),
            ToolName = name.Trim(),
            ToolType = toolType,
            Brand = brand?.Trim(),
            Model = model?.Trim(),
            Specification = specification?.Trim(),
            MaxUsageCount = maxUsageCount,
            PmIntervalCount = pmIntervalCount,
            RequiresCalibration = requiresCalibration,
            CalibrationIntervalDays = calibrationIntervalDays,
            StorageLocation = storageLocation?.Trim(),
            PurchaseDate = purchaseDate,
            PurchaseCost = purchaseCost,
            Notes = notes,
            Status = ToolStatus.Available,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(
        string name, ToolType toolType,
        string? brand, string? model, string? specification,
        int? maxUsageCount, int? pmIntervalCount,
        bool requiresCalibration, int? calibrationIntervalDays,
        string? storageLocation, DateOnly? purchaseDate, decimal? purchaseCost,
        string? notes, bool isActive, string? updatedBy)
    {
        if (Status == ToolStatus.Scrapped)
            throw new DomainException($"Dụng cụ '{ToolCode}' đã thanh lý, không thể cập nhật.");
        ValidateLife(maxUsageCount, pmIntervalCount, requiresCalibration, calibrationIntervalDays);

        ToolName = name.Trim();
        ToolType = toolType;
        Brand = brand?.Trim();
        Model = model?.Trim();
        Specification = specification?.Trim();
        MaxUsageCount = maxUsageCount;
        PmIntervalCount = pmIntervalCount;
        RequiresCalibration = requiresCalibration;
        CalibrationIntervalDays = calibrationIntervalDays;
        StorageLocation = storageLocation?.Trim();
        PurchaseDate = purchaseDate;
        PurchaseCost = purchaseCost;
        Notes = notes;
        IsActive = isActive;
        Touch(updatedBy);
    }

    public ToolOperationMapping AddOperationMapping(
        string operationCode, string? productCode, bool isRequired,
        decimal usageCountPerOp, string? updatedBy)
    {
        var normalizedOp = operationCode.Trim().ToUpperInvariant();
        var normalizedProduct = productCode?.Trim().ToUpperInvariant();
        if (_operationMappings.Any(x => x.OperationCode == normalizedOp && x.ProductCode == normalizedProduct))
            throw new DomainException(
                $"Công đoạn '{normalizedOp}' đã được khai báo cho dụng cụ '{ToolCode}'" +
                (normalizedProduct is null ? "." : $" với sản phẩm '{normalizedProduct}'."));

        var mapping = ToolOperationMapping.Create(
            ToolId, normalizedOp, normalizedProduct, isRequired, usageCountPerOp);
        _operationMappings.Add(mapping);
        Touch(updatedBy);
        return mapping;
    }

    public void RemoveOperationMapping(int mappingId, string? updatedBy)
    {
        var mapping = _operationMappings.FirstOrDefault(x => x.MappingId == mappingId)
            ?? throw new EntityNotFoundException(nameof(ToolOperationMapping), mappingId);
        _operationMappings.Remove(mapping);
        Touch(updatedBy);
    }

    public ToolCheckout Checkout(
        int workCenterId, string checkedOutBy, DateTime? expectedReturnAt, string? updatedBy)
    {
        if (Status != ToolStatus.Available)
            throw new DomainException(
                $"Chỉ dụng cụ ở trạng thái Available mới được xuất cho khu vực sản xuất. Trạng thái hiện tại: {Status}.");

        var checkout = ToolCheckout.Create(ToolId, workCenterId, checkedOutBy, expectedReturnAt);
        _checkouts.Add(checkout);
        Status = ToolStatus.InUse;
        CurrentWorkCenterId = workCenterId;
        Touch(updatedBy);
        return checkout;
    }

    public void Return(string returnedBy, ToolReturnCondition condition, string? notes, string? updatedBy)
    {
        if (Status != ToolStatus.InUse)
            throw new DomainException($"Dụng cụ '{ToolCode}' không được xuất cho khu vực nào.");

        var open = _checkouts.FirstOrDefault(x => x.ReturnedAt == null)
            ?? throw new DomainException($"Không tìm thấy phiếu xuất đang mở cho dụng cụ '{ToolCode}'.");

        open.Close(returnedBy, condition, notes);
        CurrentWorkCenterId = null;

        // Good/Worn → back to the tool room; Damaged → repair queue;
        // Missing → written off (the closed checkout records who lost it).
        Status = condition switch
        {
            ToolReturnCondition.Damaged => ToolStatus.InRepair,
            ToolReturnCondition.Missing => ToolStatus.Scrapped,
            _ => ToolStatus.Available,
        };
        if (condition == ToolReturnCondition.Missing)
            IsActive = false;
        Touch(updatedBy);
    }

    public void SendForService(ToolServiceType serviceType, string? updatedBy)
    {
        if (Status != ToolStatus.Available)
            throw new DomainException(
                $"Chỉ dụng cụ ở trạng thái Available mới được đưa đi hiệu chuẩn/sửa chữa. Trạng thái hiện tại: {Status}.");

        Status = serviceType == ToolServiceType.Calibration
            ? ToolStatus.InCalibration
            : ToolStatus.InRepair;
        Touch(updatedBy);
    }

    public ToolMaintenanceLog RecordMaintenance(
        ToolMaintenanceType maintenanceType, DateTime performedAt,
        string? performedBy, decimal? cost, int? nextDueCount, DateOnly? nextDueDate,
        string? notes, string? updatedBy)
    {
        if (Status == ToolStatus.Scrapped)
            throw new DomainException($"Dụng cụ '{ToolCode}' đã thanh lý, không thể ghi nhận bảo trì.");
        if (Status == ToolStatus.InUse)
            throw new DomainException($"Phải trả dụng cụ '{ToolCode}' về kho trước khi ghi nhận bảo trì.");

        var log = ToolMaintenanceLog.Create(
            ToolId, maintenanceType, CurrentUsageCount,
            performedAt, performedBy, cost, nextDueCount, nextDueDate, notes);
        _maintenanceLogs.Add(log);

        if (maintenanceType == ToolMaintenanceType.Reconditioning)
            UsageCountAtLastPm = CurrentUsageCount;

        if (maintenanceType == ToolMaintenanceType.Calibration)
        {
            LastCalibratedAt = performedAt;
            NextCalibrationDue = nextDueDate
                ?? (CalibrationIntervalDays is int days
                    ? DateOnly.FromDateTime(performedAt).AddDays(days)
                    : null);
        }

        if (Status is ToolStatus.InCalibration or ToolStatus.InRepair)
            Status = ToolStatus.Available;
        Touch(updatedBy);
        return log;
    }

    public void AccumulateUsage(int count, string? updatedBy)
    {
        if (Status is not (ToolStatus.Available or ToolStatus.InUse))
            throw new DomainException(
                $"Chỉ ghi nhận lượt sử dụng cho dụng cụ đang hoạt động. Trạng thái hiện tại: {Status}.");

        CurrentUsageCount += count;
        Touch(updatedBy);

        if (IsReconditioningDue)
            RaiseDomainEvent(new ToolReconditioningDueEvent(
                ToolId, ToolCode, CurrentUsageCount, UsageCountAtLastPm, PmIntervalCount!.Value));
        if (IsNearingEndOfLife)
            RaiseDomainEvent(new ToolNearingEndOfLifeEvent(
                ToolId, ToolCode, CurrentUsageCount, MaxUsageCount!.Value));
    }

    public void Scrap(string? updatedBy)
    {
        if (Status == ToolStatus.Scrapped)
            throw new DomainException($"Dụng cụ '{ToolCode}' đã được thanh lý trước đó.");
        if (Status == ToolStatus.InUse)
            throw new DomainException($"Phải trả dụng cụ '{ToolCode}' về kho trước khi thanh lý.");

        Status = ToolStatus.Scrapped;
        IsActive = false;
        Touch(updatedBy);
    }

    public bool IsReconditioningDue =>
        PmIntervalCount is int interval && CurrentUsageCount - UsageCountAtLastPm >= interval;

    public bool IsNearingEndOfLife =>
        MaxUsageCount is int max && CurrentUsageCount >= max * EndOfLifeWarningRatio;

    private static void ValidateLife(
        int? maxUsageCount, int? pmIntervalCount, bool requiresCalibration, int? calibrationIntervalDays)
    {
        if (maxUsageCount is <= 0)
            throw new DomainException("Số lượt sử dụng tối đa (MaxUsageCount) phải lớn hơn 0.");
        if (pmIntervalCount is <= 0)
            throw new DomainException("Chu kỳ tái tạo (PmIntervalCount) phải lớn hơn 0.");
        if (calibrationIntervalDays is <= 0)
            throw new DomainException("Chu kỳ hiệu chuẩn (ngày) phải lớn hơn 0.");
        if (requiresCalibration && calibrationIntervalDays is null)
            throw new DomainException("Dụng cụ yêu cầu hiệu chuẩn phải có chu kỳ hiệu chuẩn (ngày).");
    }
}
