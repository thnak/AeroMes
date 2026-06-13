using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Commands.GenerateVariantMatrix;

public sealed record GenerateVariantMatrixCommand(
    string FamilyCode,
    string ProductCodePrefix,
    string CreatedBy) : ICommand<ValidationResult<int>>;
