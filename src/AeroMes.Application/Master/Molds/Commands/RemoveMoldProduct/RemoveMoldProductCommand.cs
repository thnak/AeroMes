using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.RemoveMoldProduct;

public record RemoveMoldProductCommand(
    string MoldCode,
    int MappingId,
    string? UpdatedBy) : ICommand;
