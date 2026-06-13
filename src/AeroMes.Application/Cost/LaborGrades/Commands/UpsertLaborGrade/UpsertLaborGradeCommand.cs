using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.LaborGrades.Commands.UpsertLaborGrade;

public record UpsertLaborGradeCommand(
    string GradeCode,
    string GradeName,
    decimal HourlyRate,
    decimal OvertimeMultiplier,
    DateOnly EffectiveFrom,
    string Currency,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
