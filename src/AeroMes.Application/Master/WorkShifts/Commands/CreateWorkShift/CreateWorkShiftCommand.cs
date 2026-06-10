using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Commands.CreateWorkShift;

public record BreakPeriodInput(TimeOnly BreakStart, TimeOnly BreakEnd);

public record CreateWorkShiftCommand(
    string Code,
    string Name,
    TimeOnly StartTime,
    TimeOnly EndTime,
    IReadOnlyList<BreakPeriodInput> Breaks,
    string? CreatedBy) : ICommand<int>;
