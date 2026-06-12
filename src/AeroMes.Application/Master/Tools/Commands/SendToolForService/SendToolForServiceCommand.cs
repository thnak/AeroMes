using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.SendToolForService;

public record SendToolForServiceCommand(
    string ToolCode,
    ToolServiceType ServiceType,
    string? UpdatedBy) : ICommand;
