using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.ChangeLifecycleStatus;

public record ChangeLifecycleStatusCommand(
    string Code,
    LifecycleStatus Status,
    string UpdatedBy) : ICommand;
