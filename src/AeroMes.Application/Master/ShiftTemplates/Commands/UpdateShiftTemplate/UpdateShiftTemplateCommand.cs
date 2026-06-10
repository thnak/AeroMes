using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.UpdateShiftTemplate;

public record UpdateShiftTemplateCommand(
    string Code,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsNightShift,
    WeekDays ValidDays,
    int? WorkCenterId,
    bool IsActive,
    string? UpdatedBy) : ICommand;
