using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public enum ProductionProcessType { Production, Disassembly }
public enum ProcessApplicationScope { All, ProductGroup, SpecificProduct }
public enum StageCapacityType { Machine, MachineGroup, Team, TeamGroup }
public enum PlannedTimeSource { Automatic, Manual }

public class ProductionProcess : AuditableEntity
{
    public int ProcessID { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ProductionProcessType ProcessType { get; private set; }
    public DateOnly EffectiveDate { get; private set; }
    public ProcessApplicationScope ApplicationScope { get; private set; } = ProcessApplicationScope.All;
    public string? ProductGroupIdsJson { get; private set; }
    public string? ProductIdsJson { get; private set; }
    public bool IsForPlanningOnly { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<ProductionProcessStage> _stages = [];
    public IReadOnlyList<ProductionProcessStage> Stages => _stages.AsReadOnly();

    private ProductionProcess() { }

    public static ProductionProcess Create(
        string code, string name, ProductionProcessType processType,
        DateOnly effectiveDate, ProcessApplicationScope scope,
        string? productGroupIdsJson, string? productIdsJson,
        bool isForPlanningOnly, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Mã quy trình không được để trống.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên quy trình không được để trống.");
        return new ProductionProcess
        {
            Code = code.Trim(), Name = name.Trim(), ProcessType = processType,
            EffectiveDate = effectiveDate, ApplicationScope = scope,
            ProductGroupIdsJson = productGroupIdsJson, ProductIdsJson = productIdsJson,
            IsForPlanningOnly = isForPlanningOnly, CreatedBy = createdBy
        };
    }

    public void AddStage(ProductionProcessStage stage)
    {
        _stages.Add(stage);
    }

    public void ReplaceStages(IEnumerable<ProductionProcessStage> stages)
    {
        _stages.Clear();
        _stages.AddRange(stages);
        ValidateStages();
    }

    public void Update(
        string name, DateOnly effectiveDate, ProcessApplicationScope scope,
        string? productGroupIdsJson, string? productIdsJson,
        bool isForPlanningOnly, string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Tên quy trình không được để trống.");
        Name = name.Trim();
        EffectiveDate = effectiveDate;
        ApplicationScope = scope;
        ProductGroupIdsJson = productGroupIdsJson;
        ProductIdsJson = productIdsJson;
        IsForPlanningOnly = isForPlanningOnly;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate(string? updatedBy) { IsActive = true; Touch(updatedBy); }
    public void Deactivate(string? updatedBy) { IsActive = false; Touch(updatedBy); }

    private void ValidateStages()
    {
        if (_stages.Count == 0) throw new DomainException("Quy trình phải có ít nhất một công đoạn.");
        if (_stages.Count(s => s.IsPrimaryStage) != 1)
            throw new DomainException("Quy trình phải có đúng một công đoạn chính.");
    }
}

public class ProductionProcessStage : Entity
{
    public int StageID { get; private set; }
    public int ProcessID { get; private set; }
    public int SortOrder { get; private set; }
    public string? ProcessStepCode { get; private set; }
    public StageCapacityType CapacityType { get; private set; }
    public string CapacityIdsJson { get; private set; } = "[]";
    public decimal PlannedTimeSeconds { get; private set; }
    public PlannedTimeSource PlannedTimeSource { get; private set; } = PlannedTimeSource.Manual;
    public int TimeOffsetDays { get; private set; }
    public bool IsPrimaryStage { get; private set; }

    private ProductionProcessStage() { }

    public static ProductionProcessStage Create(
        int processId, int sortOrder, string? processStepCode,
        StageCapacityType capacityType, string capacityIdsJson,
        decimal plannedTimeSeconds, PlannedTimeSource timeSource,
        int timeOffsetDays, bool isPrimary)
        => new()
        {
            ProcessID = processId, SortOrder = sortOrder,
            ProcessStepCode = processStepCode, CapacityType = capacityType,
            CapacityIdsJson = capacityIdsJson, PlannedTimeSeconds = plannedTimeSeconds,
            PlannedTimeSource = timeSource, TimeOffsetDays = timeOffsetDays,
            IsPrimaryStage = isPrimary
        };
}
