using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.MachineEnergyProfiles.Commands.UpsertMachineEnergyProfile;

public class UpsertMachineEnergyProfileHandler(
    IMachineEnergyProfileRepository repository,
    IValidator<UpsertMachineEnergyProfileCommand> validator)
    : ICommandHandler<UpsertMachineEnergyProfileCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        UpsertMachineEnergyProfileCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        try
        {
            var existing = await repository.GetActiveByMachineAsync(command.MachineCode, ct);
            if (existing is not null)
            {
                if (existing.EffectiveFrom >= command.EffectiveFrom)
                    return ValidationResult<int>.Failure("Ngày hiệu lực mới phải sau ngày hiệu lực hiện tại.");
                existing.ExpireOn(command.EffectiveFrom.AddDays(-1));
            }

            var profile = MachineEnergyProfile.Create(
                command.MachineCode, command.NominalKW, command.LoadFactor,
                command.TariffID, command.EffectiveFrom);

            var id = await repository.AddAsync(profile, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
