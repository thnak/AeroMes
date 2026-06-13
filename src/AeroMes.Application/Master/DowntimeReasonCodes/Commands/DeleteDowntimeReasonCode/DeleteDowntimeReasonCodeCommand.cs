using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.DeleteDowntimeReasonCode;

public record DeleteDowntimeReasonCodeCommand(string Code, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
