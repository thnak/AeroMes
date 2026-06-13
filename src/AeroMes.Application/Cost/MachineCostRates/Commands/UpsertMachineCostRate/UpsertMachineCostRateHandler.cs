using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.MachineCostRates.Commands.UpsertMachineCostRate;

public class UpsertMachineCostRateHandler(
    IMachineCostRateRepository repository,
    IValidator<UpsertMachineCostRateCommand> validator)
    : ICommandHandler<UpsertMachineCostRateCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        UpsertMachineCostRateCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repository.HasOverlapAsync(command.MachineCode, command.RateType, command.EffectiveFrom, null, null, ct))
        {
            // expire the overlapping active record first
            var active = await repository.GetActiveAsync(command.MachineCode, command.RateType, ct);
            if (active is not null)
            {
                if (active.EffectiveFrom >= command.EffectiveFrom)
                    return ValidationResult<int>.Failure("Ngày hiệu lực mới phải sau ngày hiệu lực hiện tại.");
                active.ExpireOn(command.EffectiveFrom.AddDays(-1));
            }
        }

        try
        {
            var rate = MachineCostRate.Create(
                command.MachineCode, command.RateType, command.RatePerHour,
                command.EffectiveFrom, command.Notes, command.CreatedBy);

            var id = await repository.AddAsync(rate, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
