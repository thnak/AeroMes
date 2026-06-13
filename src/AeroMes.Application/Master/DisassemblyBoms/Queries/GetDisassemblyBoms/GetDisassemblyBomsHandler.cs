using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Queries.GetDisassemblyBoms;

public class GetDisassemblyBomsHandler(IDisassemblyBomRepository repo)
    : IQueryHandler<GetDisassemblyBomsQuery, IReadOnlyList<DisassemblyBomSummaryDto>>
{
    public async Task<IReadOnlyList<DisassemblyBomSummaryDto>> HandleAsync(
        GetDisassemblyBomsQuery query, CancellationToken ct)
    {
        DisassemblyBomStatus? statusFilter = query.Status is not null &&
            Enum.TryParse<DisassemblyBomStatus>(query.Status, ignoreCase: true, out var parsed)
                ? parsed
                : null;

        var items = await repo.GetAllAsync(query.SourceProductCode, statusFilter, ct);

        return items
            .Select(b => new DisassemblyBomSummaryDto(
                b.DisassemblyBomId,
                b.BomCode,
                b.BomName,
                b.SourceProductCode,
                b.SourceProduct?.ProductName,
                b.BomType.ToString(),
                b.LossRatio,
                b.IsDefault,
                b.Status.ToString(),
                b.EffectiveDate,
                b.ExpiryDate,
                b.CreatedAt))
            .ToList();
    }
}
