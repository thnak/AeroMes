using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Molds.Commands.RemoveMoldProduct;

public record RemoveMoldProductCommand(
    string MoldCode,
    int MappingId,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
