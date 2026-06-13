using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.CreateDowntimeReasonCode;

public record CreateDowntimeReasonCodeCommand(
    string Code,
    string Name,
    DowntimeCategory Category,
    int? SlaMinutes,
    bool RequiresApproval,
    string? CreatedBy) : ICommand<ValidationResult<string>>;
