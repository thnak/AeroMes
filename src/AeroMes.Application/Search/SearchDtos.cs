namespace AeroMes.Application.Search;

public record SearchDocument(
    string EntityType,
    string Title,
    string? Subtitle,
    string? Code,
    string? Tags,
    string? Description,
    string? RequiredPermission);

public record SearchResultDto(
    string EntityType,
    string Id,
    string Title,
    string? Subtitle,
    string? Code,
    IReadOnlyList<string> Highlights);

public record SearchResultPageDto(
    string Query,
    int TotalHits,
    int Page,
    int PageSize,
    IReadOnlyList<SearchResultDto> Results);

public static class SearchIndexNames
{
    public const string Products = "aeromes_products";
    public const string WorkOrders = "aeromes_work_orders";
    public const string Customers = "aeromes_customers";
    public const string Employees = "aeromes_employees";
    public const string ProductionOrders = "aeromes_production_orders";
    public const string StorageLocations = "aeromes_storage_locations";

    public static readonly string[] All =
    [
        Products, WorkOrders, Customers, Employees, ProductionOrders, StorageLocations
    ];
}
