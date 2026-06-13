using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Routings.Commands.CreateRouting;

public class CreateRoutingHandler(
    IRoutingRepository repo,
    IUnitOfWork uow,
    IValidator<CreateRoutingCommand> validator) : ICommandHandler<CreateRoutingCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateRoutingCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = Routing.Create(cmd.Code, cmd.Name, cmd.ProductCode, cmd.IsDefault, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.RoutingID);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
