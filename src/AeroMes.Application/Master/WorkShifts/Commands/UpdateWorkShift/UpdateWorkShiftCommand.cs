using AeroMes.Application.Master.WorkShifts.Commands.CreateWorkShift;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Commands.UpdateWorkShift;

public record UpdateWorkShiftCommand(
    int WorkShiftId,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    IReadOnlyList<BreakPeriodInput> Breaks,
    bool IsActive,
    string? UpdatedBy) : ICommand;
