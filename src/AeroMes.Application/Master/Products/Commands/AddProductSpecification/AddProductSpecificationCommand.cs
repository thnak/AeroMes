using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Products.Commands.AddProductSpecification;

public record AddProductSpecificationCommand(
    string ProductCode,
    string SpecCode,
    string? Description,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
