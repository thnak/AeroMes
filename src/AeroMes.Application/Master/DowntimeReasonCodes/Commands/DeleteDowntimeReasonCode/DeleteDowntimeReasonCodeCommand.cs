using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.DeleteDowntimeReasonCode;

public record DeleteDowntimeReasonCodeCommand(string Code, string? DeletedBy = null) : ICommand;
