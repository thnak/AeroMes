using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkShifts.Commands.DeleteWorkShift;

public record DeleteWorkShiftCommand(int WorkShiftId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
