using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionVouchers.Queries.GetInspectionVouchers;

public record GetInspectionVouchersQuery(
    string? Status,
    string? InspectionType,
    DateOnly? From,
    DateOnly? To,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<QualityInspectionVoucherDto>>;
