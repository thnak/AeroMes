using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Customers.Queries.GetCustomerById;

public class GetCustomerByIdHandler(ICustomerRepository repo)
    : IQueryHandler<GetCustomerByIdQuery, CustomerDetailDto?>
{
    public async Task<CustomerDetailDto?> HandleAsync(GetCustomerByIdQuery query, CancellationToken ct)
    {
        var c = await repo.GetByIdWithDetailsAsync(query.Code, ct);
        if (c is null) return null;

        var partNumbers = c.PartNumbers
            .OrderBy(x => x.CustomerPartNo)
            .Select(x => new CustomerPartNumberDto(
                x.CustomerPartNumberId, x.CustomerPartNo,
                x.ProductCode, x.Product?.ProductName,
                x.Description, x.DrawingReference, x.Revision))
            .ToList();

        var qualitySpecs = c.QualitySpecs
            .OrderBy(x => x.ProductCode)
            .Select(x => new CustomerQualitySpecDto(
                x.CustomerQualitySpecId, x.ProductCode,
                x.Product?.ProductName,
                x.AqlLevel, x.InspectionLevel?.ToString(),
                x.AcceptanceCriteria, x.MaxDefectsPpm,
                x.SpecialRequirements,
                x.EffectiveFrom, x.EffectiveTo))
            .ToList();

        return new CustomerDetailDto(
            c.CustomerCode, c.CustomerName,
            c.CustomerType.ToString(),
            c.TaxId, c.Country, c.Address, c.ShippingAddress,
            c.ContactName, c.ContactPhone, c.ContactEmail,
            c.CreditTermsDays, c.Currency,
            c.IsActive, c.Notes,
            partNumbers, qualitySpecs);
    }
}
