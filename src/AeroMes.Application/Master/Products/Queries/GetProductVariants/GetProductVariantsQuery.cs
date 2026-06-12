using AeroMes.Domain.Master;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Products.Queries.GetProductVariants;

public record GetProductVariantsQuery(string ParentProductCode) : IQuery<IReadOnlyList<ProductVariantDto>>;

public record ProductVariantDto(
    string ProductCode,
    string ProductName,
    LifecycleStatus LifecycleStatus,
    bool IsActive);
