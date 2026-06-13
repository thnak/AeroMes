using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.CreateShiftTemplate;

public record CreateShiftTemplateCommand(
    string Code,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsNightShift,
    WeekDays ValidDays,
    int? WorkCenterId,
    string? CreatedBy) : ICommand<ValidationResult<string>>;
