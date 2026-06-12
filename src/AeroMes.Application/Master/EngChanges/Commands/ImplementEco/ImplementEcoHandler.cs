using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.ImplementEco;

public class ImplementEcoHandler(
    IEngChangeRepository ecRepo,
    IBomHeaderRepository bomRepo,
    IUnitOfWork uow) : ICommandHandler<ImplementEcoCommand, ImplementEcoResult>
{
    public async Task<ImplementEcoResult> HandleAsync(ImplementEcoCommand cmd, CancellationToken ct)
    {
        var ec = await ecRepo.GetByNumberAsync(cmd.EcNumber, ct)
            ?? throw new EntityNotFoundException(nameof(EngChange), cmd.EcNumber);

        // Validate before creating the draft — the header is saved first (to get its ID),
        // so an ineligible ECO must fail before any side effects.
        ec.EnsureCanImplement();

        var header = BomHeader.Create(
            cmd.ProductCode, cmd.NewVersion,
            baseQuantity: 1m, ecoReference: ec.EcNumber, notes: ec.Title, cmd.UpdatedBy);

        if (cmd.CloneFromActive)
        {
            var active = await bomRepo.GetActiveByProductAsync(cmd.ProductCode, ct);
            if (active is not null)
            {
                header = BomHeader.Create(
                    cmd.ProductCode, cmd.NewVersion,
                    active.BaseQuantity, ec.EcNumber, ec.Title, cmd.UpdatedBy);
                header.CloneLinesFrom(active);
            }
        }

        await bomRepo.AddAsync(header, ct);
        await uow.SaveChangesAsync(ct); // assigns BomHeaderId before linking it on the ECO

        ec.MarkImplemented(header.BomHeaderId, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);

        return new ImplementEcoResult(header.BomHeaderId, header.ProductCode, header.Version);
    }
}
