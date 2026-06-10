using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Commands.DeleteWorkShift;

public record DeleteWorkShiftCommand(int WorkShiftId, string? DeletedBy) : ICommand;
