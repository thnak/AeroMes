using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Commands.AddVariantDimension;

public sealed record AddVariantDimensionCommand(
    string FamilyCode,
    string DimensionName,
    int SortOrder,
    bool IsRequired,
    string CreatedBy) : ICommand<ValidationResult<int>>;
