using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.UpdateDowntimeReasonCode;

public record UpdateDowntimeReasonCodeCommand(
    string Code,
    string Name,
    DowntimeCategory Category,
    int? SlaMinutes,
    bool RequiresApproval,
    bool IsActive,
    string? UpdatedBy) : ICommand;
