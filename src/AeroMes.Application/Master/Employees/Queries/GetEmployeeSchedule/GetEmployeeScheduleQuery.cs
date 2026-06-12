using AeroMes.Application.Master.Employees.Queries.GetEmployeeById;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Employees.Queries.GetEmployeeSchedule;

/// <summary>Shift assignments effective on <paramref name="AsOf"/> (today when omitted).</summary>
public record GetEmployeeScheduleQuery(string Code, DateOnly? AsOf = null)
    : IQuery<EmployeeScheduleDto?>;

public record EmployeeScheduleDto(
    string EmployeeCode,
    string FullName,
    DateOnly AsOf,
    IReadOnlyList<ShiftAssignmentDto> Assignments);
