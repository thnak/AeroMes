using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.AddToolOperation;

public class AddToolOperationHandler(
    IToolRepository repo,
    IUnitOfWork uow,
    IValidator<AddToolOperationCommand> validator) : ICommandHandler<AddToolOperationCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddToolOperationCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
                ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

            var mapping = tool.AddOperationMapping(
                cmd.OperationCode, cmd.ProductCode, cmd.IsRequired, cmd.UsageCountPerOp, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(mapping.MappingId);
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
