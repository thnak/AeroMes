using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.UnmountMold;

public record UnmountMoldCommand(string MoldCode, string? UpdatedBy) : ICommand;
