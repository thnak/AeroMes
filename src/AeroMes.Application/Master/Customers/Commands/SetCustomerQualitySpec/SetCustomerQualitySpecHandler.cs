using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

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
            var customer = await repo.GetByIdWithDetailsAsync(cmd.CustomerCode, ct);
            if (customer is null) return ValidationResult<int>.NotFound($"Entity '{cmd.CustomerCode}' was not found.");
            var spec = customer.SetQualitySpec(
                cmd.ProductCode,
                cmd.AqlLevel, cmd.InspectionLevel,
                cmd.AcceptanceCriteria, cmd.MaxDefectsPpm,
                cmd.SpecialRequirements,
                cmd.EffectiveFrom, cmd.EffectiveTo);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(spec.CustomerQualitySpecId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
