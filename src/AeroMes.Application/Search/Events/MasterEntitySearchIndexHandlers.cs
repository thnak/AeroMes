using AeroMes.Domain.Master.Events;
using LiteBus.Events.Abstractions;

namespace AeroMes.Application.Search.Events;

public class ProductSavedIndexHandler(ISearchIndexer indexer) : IEventHandler<ProductSavedEvent>
{
    public Task HandleAsync(ProductSavedEvent e, CancellationToken ct) =>
        indexer.UpsertAsync(SearchIndexNames.Products, e.ProductCode,
            new SearchDocument("product", e.ProductName, e.ProductCode, e.ProductCode, null, null, "master:read"), ct);
}

public class ProductDeletedIndexHandler(ISearchIndexer indexer) : IEventHandler<ProductDeletedEvent>
{
    public Task HandleAsync(ProductDeletedEvent e, CancellationToken ct) =>
        indexer.DeleteAsync(SearchIndexNames.Products, e.ProductCode, ct);
}

public class CustomerSavedIndexHandler(ISearchIndexer indexer) : IEventHandler<CustomerSavedEvent>
{
    public Task HandleAsync(CustomerSavedEvent e, CancellationToken ct) =>
        indexer.UpsertAsync(SearchIndexNames.Customers, e.CustomerCode,
            new SearchDocument("customer", e.CustomerName, e.CustomerCode, e.CustomerCode,
                e.TaxId, e.Address, "master:read"), ct);
}

public class CustomerDeletedIndexHandler(ISearchIndexer indexer) : IEventHandler<CustomerDeletedEvent>
{
    public Task HandleAsync(CustomerDeletedEvent e, CancellationToken ct) =>
        indexer.DeleteAsync(SearchIndexNames.Customers, e.CustomerCode, ct);
}

public class EmployeeSavedIndexHandler(ISearchIndexer indexer) : IEventHandler<EmployeeSavedEvent>
{
    public Task HandleAsync(EmployeeSavedEvent e, CancellationToken ct) =>
        indexer.UpsertAsync(SearchIndexNames.Employees, e.EmployeeCode,
            new SearchDocument("employee", e.FullName, e.EmployeeCode, e.EmployeeCode,
                e.Department, null, "master:read"), ct);
}

public class EmployeeDeletedIndexHandler(ISearchIndexer indexer) : IEventHandler<EmployeeDeletedEvent>
{
    public Task HandleAsync(EmployeeDeletedEvent e, CancellationToken ct) =>
        indexer.DeleteAsync(SearchIndexNames.Employees, e.EmployeeCode, ct);
}
