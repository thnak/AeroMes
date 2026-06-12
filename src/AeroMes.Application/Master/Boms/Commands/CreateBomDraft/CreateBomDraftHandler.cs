using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.CreateBomDraft;

public class CreateBomDraftHandler(
    IBomHeaderRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateBomDraftCommand, int>
{
    public async Task<int> HandleAsync(CreateBomDraftCommand cmd, CancellationToken ct)
    {
        var header = BomHeader.Create(
            cmd.ProductCode, cmd.Version, cmd.BaseQuantity, null, cmd.Notes, cmd.CreatedBy);

        if (cmd.CloneFromVersion is not null)
        {
            var source = await repo.GetByProductAndVersionAsync(cmd.ProductCode, cmd.CloneFromVersion, ct)
                ?? throw new EntityNotFoundException(nameof(BomHeader), $"{cmd.ProductCode}/{cmd.CloneFromVersion}");
            header.CloneLinesFrom(source);
        }

        await repo.AddAsync(header, ct);
        await uow.SaveChangesAsync(ct);
        return header.BomHeaderId;
    }
}
