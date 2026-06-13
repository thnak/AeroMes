using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Tools.Commands.SendToolForService;

public class SendToolForServiceHandler(
    IToolRepository repo,
    IUnitOfWork uow,
    IValidator<SendToolForServiceCommand> validator) : ICommandHandler<SendToolForServiceCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(SendToolForServiceCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct)
                ?? throw new EntityNotFoundException(nameof(Tool), cmd.ToolCode);

            tool.SendForService(cmd.ServiceType, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
