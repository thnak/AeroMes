using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.DisassemblyBoms.Queries.GetDisassemblyBomById;

public class GetDisassemblyBomByIdHandler(IDisassemblyBomRepository repo)
    : IQueryHandler<GetDisassemblyBomByIdQuery, DisassemblyBomDetailDto?>
{
    public async Task<DisassemblyBomDetailDto?> HandleAsync(
        GetDisassemblyBomByIdQuery query, CancellationToken ct)
    {
        var entity = await repo.GetByIdWithLinesAsync(query.DisassemblyBomId, ct);
        if (entity is null)
            return null;

        var lines = entity.Lines
            .Select(l => new DisassemblyBomLineDto(
                l.LineId,
                l.LineNo,
                l.ComponentCode,
                l.Component?.ProductName,
                l.ComponentType.ToString(),
                l.RecoveryRate,
                l.FixedQuantity,
                l.UoMCode,
                l.Notes))
            .ToList();

        return new DisassemblyBomDetailDto(
            entity.DisassemblyBomId,
            entity.BomCode,
            entity.BomName,
            entity.SourceProductCode,
            entity.SourceProduct?.ProductName,
            entity.BomType.ToString(),
            entity.LossRatio,
            entity.IsDefault,
            entity.Status.ToString(),
            entity.EffectiveDate,
            entity.ExpiryDate,
            entity.CreatedAt,
            lines);
    }
}
