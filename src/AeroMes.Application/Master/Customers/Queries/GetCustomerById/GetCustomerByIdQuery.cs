using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(string Code) : IQuery<CustomerDetailDto?>;

public record CustomerDetailDto(
    string CustomerCode,
    string CustomerName,
    string CustomerType,
    string? TaxId,
    string? Country,
    string? Address,
    string? ShippingAddress,
    string? ContactName,
    string? ContactPhone,
    string? ContactEmail,
    int CreditTermsDays,
    string Currency,
    bool IsActive,
    string? Notes,
    IReadOnlyList<CustomerPartNumberDto> PartNumbers,
    IReadOnlyList<CustomerQualitySpecDto> QualitySpecs);

public record CustomerPartNumberDto(
    int CustomerPartNumberId,
    string CustomerPartNo,
    string ProductCode,
    string? ProductName,
    string? Description,
    string? DrawingReference,
    string? Revision);

public record CustomerQualitySpecDto(
    int CustomerQualitySpecId,
    string ProductCode,
    string? ProductName,
    string? AqlLevel,
    string? InspectionLevel,
    string? AcceptanceCriteria,
    int? MaxDefectsPpm,
    string? SpecialRequirements,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo);
