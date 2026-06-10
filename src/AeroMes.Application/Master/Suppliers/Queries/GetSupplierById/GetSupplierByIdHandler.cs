using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Queries.GetSupplierById;

public class GetSupplierByIdHandler(ISupplierRepository repo)
    : IQueryHandler<GetSupplierByIdQuery, SupplierDetailDto?>
{
    public async Task<SupplierDetailDto?> HandleAsync(GetSupplierByIdQuery query, CancellationToken ct)
    {
        var s = await repo.GetByIdWithAvlAsync(query.Code, ct);
        if (s is null) return null;

        var avlItems = s.AvlItems
            .OrderBy(x => x.ProductCode)
            .Select(x => new AvlItemDto(
                x.AvlItemId, x.ProductCode,
                x.Product?.ProductName,
                x.Status.ToString(),
                x.UnitPrice, x.CurrencyCode, x.LeadTimeDays,
                x.MinOrderQty, x.AqlLevel, x.IsPreferred,
                x.ApprovedFrom, x.ApprovedTo, x.Notes))
            .ToList();

        return new SupplierDetailDto(
            s.SupplierCode, s.SupplierName,
            s.Country, s.City, s.Address,
            s.Phone, s.Email, s.ContactName, s.TaxCode,
            s.IsActive, avlItems);
    }
}
