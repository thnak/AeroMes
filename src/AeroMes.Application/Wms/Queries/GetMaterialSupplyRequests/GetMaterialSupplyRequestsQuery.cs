using AeroMes.Domain.Wms;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetMaterialSupplyRequests;

public record GetMaterialSupplyRequestsQuery(
    MaterialSupplyRequestType? RequestType,
    MaterialSupplyRequestStatus? Status
) : IQuery<IReadOnlyList<MaterialSupplyRequestSummaryDto>>;

public record MaterialSupplyRequestSummaryDto(
    int RequestId,
    string VoucherNumber,
    MaterialSupplyRequestType RequestType,
    MaterialSupplyRequestStatus Status,
    string RequesterUnit,
    DateTime? RequiredByDate,
    string? Notes,
    int LineCount,
    DateTime CreatedAt,
    string? CreatedBy);
