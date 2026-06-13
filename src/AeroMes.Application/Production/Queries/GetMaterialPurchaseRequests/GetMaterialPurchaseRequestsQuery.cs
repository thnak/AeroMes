using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetMaterialPurchaseRequests;

public record GetMaterialPurchaseRequestsQuery(
    PurchaseRequestStatus? Status,
    PurchaseRequestSourceType? SourceType,
    string? RequestingUnit,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<MaterialPurchaseRequestDto>>;
