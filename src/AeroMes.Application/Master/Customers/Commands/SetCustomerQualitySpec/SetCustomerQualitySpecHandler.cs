using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Customers.Commands.SetCustomerQualitySpec;

public class SetCustomerQualitySpecHandler(
    ICustomerRepository repo,
    IUnitOfWork uow,
    IValidator<SetCustomerQualitySpecCommand> validator) : ICommandHandler<SetCustomerQualitySpecCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(SetCustomerQualitySpecCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
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
            return ValidationResult<int>.Ok(spec.CustomerQualitySpecId);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<int>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
