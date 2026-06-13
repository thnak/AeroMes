using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductFamilies.Commands.AddDimensionValue;

public sealed record AddDimensionValueCommand(
    string FamilyCode,
    int DimensionId,
    string ValueCode,
    string ValueLabel,
    int SortOrder,
    string CreatedBy) : ICommand<ValidationResult<int>>;
