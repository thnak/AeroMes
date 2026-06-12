using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.AddMoldProduct;

public class AddMoldProductHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<AddMoldProductCommand, int>
{
    public async Task<int> HandleAsync(AddMoldProductCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.MoldCode);

        var mapping = mold.AddProductMapping(
            cmd.ProductCode, cmd.IsDefault, cmd.CycleTimeSeconds, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
        return mapping.MappingId;
    }
}
