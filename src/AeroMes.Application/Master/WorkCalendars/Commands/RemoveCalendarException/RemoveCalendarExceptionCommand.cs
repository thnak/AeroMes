using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.RemoveCalendarException;

public record RemoveCalendarExceptionCommand(int WorkCalendarId, int ExceptionId, string? DeletedBy) : ICommand;
