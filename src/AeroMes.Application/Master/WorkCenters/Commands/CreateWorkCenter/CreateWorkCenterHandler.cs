using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Master;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkCenters.Commands.CreateWorkCenter;
public class CreateWorkCenterHandler(
    IWorkCenterRepository repo,
    IUnitOfWork uow,
    IValidator<CreateWorkCenterCommand> validator) : ICommandHandler<CreateWorkCenterCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateWorkCenterCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());
        try
        {
            var entity = WorkCenter.Create(cmd.Code, cmd.Name, cmd.Description, cmd.CreatedBy);
            await repo.AddAsync(entity, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(entity.WorkCenterID);
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
