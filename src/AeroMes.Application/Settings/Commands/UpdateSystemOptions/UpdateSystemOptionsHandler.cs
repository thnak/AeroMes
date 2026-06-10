using AeroMes.Application.Interfaces;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Settings.Commands.UpdateSystemOptions;

public class UpdateSystemOptionsHandler(ISystemOptionsRepository repo, IUnitOfWork uow)
    : ICommandHandler<UpdateSystemOptionsCommand>
{
    public async Task HandleAsync(UpdateSystemOptionsCommand cmd, CancellationToken ct)
    {
        var options = await repo.GetAsync(ct);
        options.Update(cmd.Options, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
