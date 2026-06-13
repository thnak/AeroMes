using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Products.Commands.RemoveProductSpecification;

public record RemoveProductSpecificationCommand(
    string ProductCode,
    int SpecificationId,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
