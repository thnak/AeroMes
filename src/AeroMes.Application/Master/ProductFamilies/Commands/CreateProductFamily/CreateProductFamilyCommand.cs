using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Commands.CreateProductFamily;

public sealed record CreateProductFamilyCommand(
    string FamilyCode,
    string FamilyName,
    string BaseProductCode,
    string Industry,
    string CreatedBy) : ICommand<ValidationResult<string>>;
