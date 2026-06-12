using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.ScrapMold;

public record ScrapMoldCommand(string MoldCode, string? UpdatedBy) : ICommand;
