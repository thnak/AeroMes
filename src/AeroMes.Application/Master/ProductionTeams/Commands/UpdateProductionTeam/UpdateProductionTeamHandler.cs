using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.UpdateProductionTeam;

public class UpdateProductionTeamHandler(IProductionTeamRepository repo, IUnitOfWork uow, IValidator<UpdateProductionTeamCommand> validator)
    : ICommandHandler<UpdateProductionTeamCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateProductionTeamCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var team = await repo.GetByCodeAsync(cmd.Code, ct)
                ?? throw new EntityNotFoundException(nameof(ProductionTeam), cmd.Code);

            team.UpdateDetails(
                cmd.Name, cmd.OrgUnitId,
                cmd.StandardLaborQuantity, cmd.ProductionRate,
                cmd.IsOrderBasedPlanningEnabled, cmd.IsActive,
                cmd.ProductGroupCategoryIds,
                cmd.UpdatedBy);
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
