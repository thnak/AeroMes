using AeroMes.Application.Common;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Import.Queries.GetImportHistory;

public record GetImportHistoryQuery(int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<ImportJobSummaryDto>>;
