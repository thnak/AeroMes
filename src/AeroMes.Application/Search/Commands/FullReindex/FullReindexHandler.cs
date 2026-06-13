using AeroMes.Application.Common;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Search.Commands.FullReindex;

public class FullReindexHandler(
    ISearchIndexer indexer,
    IProductRepository productRepo,
    ICustomerRepository customerRepo,
    IEmployeeRepository employeeRepo,
    IProductionOrderRepository poRepo) : ICommandHandler<FullReindexCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(FullReindexCommand cmd, CancellationToken ct = default)
    {
        var total = 0;
        var indexAll = cmd.IndexName is null;

        if (indexAll || cmd.IndexName == SearchIndexNames.Products)
        {
            await indexer.EnsureIndexAsync(SearchIndexNames.Products, ct);
            var products = await productRepo.GetAllAsync(activeOnly: false, ct);
            await indexer.BulkUpsertAsync(SearchIndexNames.Products,
                products.Select(p => (p.ProductCode, new SearchDocument(
                    "product", p.ProductName, p.ProductCode, p.ProductCode,
                    null, null, "master:read"))), ct);
            total += products.Count;
        }

        if (indexAll || cmd.IndexName == SearchIndexNames.Customers)
        {
            await indexer.EnsureIndexAsync(SearchIndexNames.Customers, ct);
            var customers = await customerRepo.GetAllAsync(activeOnly: false, ct);
            await indexer.BulkUpsertAsync(SearchIndexNames.Customers,
                customers.Select(c => (c.CustomerCode, new SearchDocument(
                    "customer", c.CustomerName, c.CustomerCode, c.CustomerCode,
                    c.TaxId, c.Address, "master:read"))), ct);
            total += customers.Count;
        }

        if (indexAll || cmd.IndexName == SearchIndexNames.Employees)
        {
            await indexer.EnsureIndexAsync(SearchIndexNames.Employees, ct);
            var employees = await employeeRepo.GetAllAsync(activeOnly: false, ct);
            await indexer.BulkUpsertAsync(SearchIndexNames.Employees,
                employees.Select(e => (e.EmployeeCode, new SearchDocument(
                    "employee", e.FullName, e.EmployeeCode, e.EmployeeCode,
                    e.Department, null, "master:read"))), ct);
            total += employees.Count;
        }

        if (indexAll || cmd.IndexName == SearchIndexNames.ProductionOrders)
        {
            await indexer.EnsureIndexAsync(SearchIndexNames.ProductionOrders, ct);
            var pos = await poRepo.GetFilteredAsync(null, null, null, null, ct);
            await indexer.BulkUpsertAsync(SearchIndexNames.ProductionOrders,
                pos.Select(p => (p.POID.ToString(), new SearchDocument(
                    "production_order", p.POCode, p.ProductCode, p.POCode,
                    p.Status.ToString(), null, "production:read"))), ct);
            total += pos.Count;
        }

        return ValidationResult<int>.Ok(total);
    }
}
