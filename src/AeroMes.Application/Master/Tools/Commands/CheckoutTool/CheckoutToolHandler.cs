using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Tools.Commands.CheckoutTool;

public class CheckoutToolHandler(
    IToolRepository repo,
    IUnitOfWork uow,
    IValidator<CheckoutToolCommand> validator) : ICommandHandler<CheckoutToolCommand, ValidationResult<long>>
{
    public async Task<ValidationResult<long>> HandleAsync(CheckoutToolCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<long>.Invalid(validation.ToErrorDictionary());

        try
        {
            var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct);
            if (tool is null) return ValidationResult<long>.NotFound($"Entity '{cmd.ToolCode}' was not found.");

            var checkout = tool.Checkout(
                cmd.WorkCenterId, cmd.CheckedOutBy, cmd.ExpectedReturnAt, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<long>.Ok(checkout.CheckoutId);
        }        catch (DomainException ex)
        {
            return ValidationResult<long>.Failure(ex.Message);
        }
    }
}
