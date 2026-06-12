using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Employees.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(string Code) : IQuery<EmployeeDetailDto?>;

public record EmployeeDetailDto(
    string EmployeeCode,
    string FullName,
    string? Department,
    string RoleType,
    int? DefaultWorkCenterId,
    string? DefaultWorkCenterName,
    bool IsActive,
    IReadOnlyList<EmployeeSkillDto> Skills,
    IReadOnlyList<ShiftAssignmentDto> ShiftAssignments);

public record EmployeeSkillDto(
    int EmployeeSkillId,
    string OperationCode,
    string? OperationName,
    int CertificationLevel,
    DateOnly CertifiedAt,
    DateOnly? ExpiresAt);

public record ShiftAssignmentDto(
    int ShiftAssignmentId,
    int WorkCenterId,
    string? WorkCenterName,
    string ShiftCode,
    string? ShiftName,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    DateOnly ValidFrom,
    DateOnly? ValidTo);
