using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.WorkCalendars.Commands.RemoveCalendarException;

public record RemoveCalendarExceptionCommand(int WorkCalendarId, int ExceptionId, string? DeletedBy) : ICommand<ValidationResult<Unit>>;
