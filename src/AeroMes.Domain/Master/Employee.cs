using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class Employee : AuditableEntity
{
    public string EmployeeCode { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? Department { get; private set; }
    public EmployeeRoleType RoleType { get; private set; } = EmployeeRoleType.Operator;
    public int? DefaultWorkCenterId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public WorkCenter? DefaultWorkCenter { get; private set; }

    private readonly List<EmployeeSkill> _skills = [];
    public IReadOnlyList<EmployeeSkill> Skills => _skills.AsReadOnly();

    private readonly List<ShiftAssignment> _shiftAssignments = [];
    public IReadOnlyList<ShiftAssignment> ShiftAssignments => _shiftAssignments.AsReadOnly();

    private Employee() { }

    public static Employee Create(
        string code, string fullName, string? department,
        EmployeeRoleType roleType, int? defaultWorkCenterId,
        string? createdBy)
    {
        return new Employee
        {
            EmployeeCode = code.Trim().ToUpperInvariant(),
            FullName = fullName.Trim(),
            Department = department?.Trim(),
            RoleType = roleType,
            DefaultWorkCenterId = defaultWorkCenterId,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(
        string fullName, string? department,
        EmployeeRoleType roleType, int? defaultWorkCenterId,
        bool isActive, string? updatedBy)
    {
        FullName = fullName.Trim();
        Department = department?.Trim();
        RoleType = roleType;
        DefaultWorkCenterId = defaultWorkCenterId;
        IsActive = isActive;
        Touch(updatedBy);
    }

    // ── Skills ──────────────────────────────────────────────────────────────

    public EmployeeSkill SetSkill(
        string operationCode, int certificationLevel,
        DateOnly certifiedAt, DateOnly? expiresAt)
    {
        if (certificationLevel is < 1 or > 5)
            throw new DomainException($"Certification level must be between 1 and 5. Got: {certificationLevel}.");

        var normalizedCode = operationCode.Trim().ToUpperInvariant();
        var existing = _skills.FirstOrDefault(x => x.OperationCode == normalizedCode);
        if (existing is not null)
        {
            existing.Update(certificationLevel, certifiedAt, expiresAt);
            return existing;
        }

        var skill = EmployeeSkill.Create(EmployeeCode, normalizedCode, certificationLevel, certifiedAt, expiresAt);
        _skills.Add(skill);
        return skill;
    }

    public void RemoveSkill(int employeeSkillId)
    {
        var skill = _skills.FirstOrDefault(x => x.EmployeeSkillId == employeeSkillId)
            ?? throw new DomainException($"employeeSkillId '{employeeSkillId}' was not found.");
        _skills.Remove(skill);
    }

    public bool IsCertifiedFor(string operationCode, DateOnly asOf)
    {
        var normalizedCode = operationCode.Trim().ToUpperInvariant();
        return _skills.Any(x => x.OperationCode == normalizedCode && x.IsValidOn(asOf));
    }

    // ── Shift assignments ───────────────────────────────────────────────────

    public ShiftAssignment AddShiftAssignment(
        int workCenterId, string shiftCode,
        DateOnly validFrom, DateOnly? validTo)
    {
        if (validTo is not null && validTo < validFrom)
            throw new DomainException("ValidTo must be on or after ValidFrom.");

        var normalizedShift = shiftCode.Trim().ToUpperInvariant();
        var overlapping = _shiftAssignments.Any(x =>
            x.WorkCenterId == workCenterId &&
            x.ShiftCode == normalizedShift &&
            x.ValidFrom <= (validTo ?? DateOnly.MaxValue) &&
            (x.ValidTo ?? DateOnly.MaxValue) >= validFrom);
        if (overlapping)
            throw new DomainException(
                $"Employee already has an overlapping assignment for shift '{normalizedShift}' at work center {workCenterId}.");

        var assignment = ShiftAssignment.Create(EmployeeCode, workCenterId, normalizedShift, validFrom, validTo);
        _shiftAssignments.Add(assignment);
        return assignment;
    }

    public void EndShiftAssignment(int shiftAssignmentId, DateOnly validTo)
    {
        var assignment = _shiftAssignments.FirstOrDefault(x => x.ShiftAssignmentId == shiftAssignmentId)
            ?? throw new DomainException($"shiftAssignmentId '{shiftAssignmentId}' was not found.");
        if (validTo < assignment.ValidFrom)
            throw new DomainException("ValidTo must be on or after ValidFrom.");
        assignment.End(validTo);
    }

    public void RemoveShiftAssignment(int shiftAssignmentId)
    {
        var assignment = _shiftAssignments.FirstOrDefault(x => x.ShiftAssignmentId == shiftAssignmentId)
            ?? throw new DomainException($"shiftAssignmentId '{shiftAssignmentId}' was not found.");
        _shiftAssignments.Remove(assignment);
    }
}
