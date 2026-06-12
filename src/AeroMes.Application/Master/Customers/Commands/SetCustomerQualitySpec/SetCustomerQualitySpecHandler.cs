using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.SetCustomerQualitySpec;

public class SetCustomerQualitySpecHandler(
    ICustomerRepository repo,
    IUnitOfWork uow) : ICommandHandler<SetCustomerQualitySpecCommand, int>
{
    public async Task<int> HandleAsync(SetCustomerQualitySpecCommand cmd, CancellationToken ct)
    {
        var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct)
            ?? throw new EntityNotFoundException(nameof(cmd.CustomerCode), cmd.CustomerCode);
        var spec = customer.SetQualitySpec(
            cmd.ProductCode,
            cmd.AqlLevel, cmd.InspectionLevel,
            cmd.AcceptanceCriteria, cmd.MaxDefectsPpm,
            cmd.SpecialRequirements,
            cmd.EffectiveFrom, cmd.EffectiveTo);
        await uow.SaveChangesAsync(ct);
        return spec.CustomerQualitySpecId;
    }
}
