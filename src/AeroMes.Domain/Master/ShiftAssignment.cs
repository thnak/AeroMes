using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class ShiftAssignment : Entity
{
    public int ShiftAssignmentId { get; private set; }
    public string EmployeeCode { get; private set; } = string.Empty;
    public int WorkCenterId { get; private set; }
    public string ShiftCode { get; private set; } = string.Empty;
    public DateOnly ValidFrom { get; private set; }
    public DateOnly? ValidTo { get; private set; }

    public WorkCenter? WorkCenter { get; private set; }
    public ShiftTemplate? ShiftTemplate { get; private set; }

    private ShiftAssignment() { }

    internal static ShiftAssignment Create(
        string employeeCode, int workCenterId, string shiftCode,
        DateOnly validFrom, DateOnly? validTo)
    {
        return new ShiftAssignment
        {
            EmployeeCode = employeeCode,
            WorkCenterId = workCenterId,
            ShiftCode = shiftCode.Trim().ToUpperInvariant(),
            ValidFrom = validFrom,
            ValidTo = validTo,
        };
    }

    internal void End(DateOnly validTo) => ValidTo = validTo;

    public bool IsActiveOn(DateOnly date) =>
        ValidFrom <= date && (ValidTo is null || ValidTo.Value >= date);
}
