using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCalendars.Commands.DeleteWorkCalendar;

public record DeleteWorkCalendarCommand(int WorkCalendarId, string? DeletedBy) : ICommand;
