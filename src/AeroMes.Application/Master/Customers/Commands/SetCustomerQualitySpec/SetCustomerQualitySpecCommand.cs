using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.SetCustomerQualitySpec;

public record SetCustomerQualitySpecCommand(
    string CustomerCode,
    string ProductCode,
    string? AqlLevel,
    InspectionLevel? InspectionLevel,
    string? AcceptanceCriteria,
    int? MaxDefectsPpm,
    string? SpecialRequirements,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    string? UpdatedBy) : ICommand<ValidationResult<int>>;
