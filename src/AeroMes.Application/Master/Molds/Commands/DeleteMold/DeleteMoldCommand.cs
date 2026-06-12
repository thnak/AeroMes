using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.DeleteMold;

public record DeleteMoldCommand(string MoldCode, string? DeletedBy) : ICommand;
