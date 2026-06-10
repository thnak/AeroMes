using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Suppliers.Queries.GetSuppliers;

public class GetSuppliersHandler(ISupplierRepository repo)
    : IQueryHandler<GetSuppliersQuery, IReadOnlyList<SupplierDto>>
{
    public async Task<IReadOnlyList<SupplierDto>> HandleAsync(GetSuppliersQuery query, CancellationToken ct)
    {
        var suppliers = await repo.GetAllAsync(query.ActiveOnly, ct);
        return suppliers
            .Select(s => new SupplierDto(
                s.SupplierCode, s.SupplierName,
                s.Country, s.City, s.Phone, s.Email, s.ContactName,
                s.IsActive, s.AvlItems.Count))
            .ToList();
    }
}
