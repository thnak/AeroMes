using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public class InspectionPlan : Entity
{
    public int PlanId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int RoutingStepId { get; private set; }
    public string? ProductCode { get; private set; }
    public string SamplingMethod { get; private set; } = string.Empty; // FULL | AQL | FIXED_N
    public int? SampleSize { get; private set; }
    public int AcceptNumber { get; private set; }
    public int RejectNumber { get; private set; }
    public string InspectionType { get; private set; } = string.Empty; // DIMENSIONAL | VISUAL | FUNCTIONAL | COMBINED
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<InspectionCharacteristic> _characteristics = [];
    public IReadOnlyCollection<InspectionCharacteristic> Characteristics => _characteristics.AsReadOnly();

    private InspectionPlan() { }

    public static InspectionPlan Create(
        string code,
        string name,
        int routingStepId,
        string? productCode,
        string samplingMethod,
        int? sampleSize,
        int acceptNumber,
        int rejectNumber,
        string inspectionType,
        string? notes,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Inspection plan code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Inspection plan name is required.");
        if (routingStepId <= 0)
            throw new DomainException("RoutingStepId must be a positive integer.");
        if (acceptNumber < 0)
            throw new DomainException("Accept number cannot be negative.");
        if (rejectNumber < 0)
            throw new DomainException("Reject number cannot be negative.");

        var now = DateTime.UtcNow;
        return new InspectionPlan
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            RoutingStepId = routingStepId,
            ProductCode = productCode?.Trim().ToUpperInvariant(),
            SamplingMethod = samplingMethod.Trim().ToUpperInvariant(),
            SampleSize = sampleSize,
            AcceptNumber = acceptNumber,
            RejectNumber = rejectNumber,
            InspectionType = inspectionType.Trim().ToUpperInvariant(),
            Notes = notes?.Trim(),
            CreatedBy = createdBy,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Update(
        string name,
        int routingStepId,
        string? productCode,
        string samplingMethod,
        int? sampleSize,
        int acceptNumber,
        int rejectNumber,
        string inspectionType,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Inspection plan name is required.");
        if (routingStepId <= 0)
            throw new DomainException("RoutingStepId must be a positive integer.");

        Name = name.Trim();
        RoutingStepId = routingStepId;
        ProductCode = productCode?.Trim().ToUpperInvariant();
        SamplingMethod = samplingMethod.Trim().ToUpperInvariant();
        SampleSize = sampleSize;
        AcceptNumber = acceptNumber;
        RejectNumber = rejectNumber;
        InspectionType = inspectionType.Trim().ToUpperInvariant();
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public void AddCharacteristic(InspectionCharacteristic characteristic)
    {
        if (characteristic is null)
            throw new DomainException("Characteristic cannot be null.");
        _characteristics.Add(characteristic);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveCharacteristic(int charId)
    {
        var item = _characteristics.FirstOrDefault(c => c.CharId == charId)
            ?? throw new DomainException($"Characteristic {charId} not found in this plan.");
        _characteristics.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }
}
