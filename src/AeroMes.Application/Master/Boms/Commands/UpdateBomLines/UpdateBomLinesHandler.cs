using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.UpdateBomLines;

public class UpdateBomLinesHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateBomLinesCommand>
{
    public async Task HandleAsync(UpdateBomLinesCommand cmd, CancellationToken ct)
    {
        var header = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.Version, ct)
            ?? throw new EntityNotFoundException(nameof(BomHeader), $"{cmd.ProductCode}/{cmd.Version}");

        header.ReplaceLines(
            cmd.Lines
                .Select(l => (l.LineNo, l.ComponentCode, l.RequiredQty, l.UoMCode,
                    l.ScrapFactor, l.IsPhantom, l.Notes))
                .ToList(),
            cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
