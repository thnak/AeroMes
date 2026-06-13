using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.CreateProductionTeam;

public class CreateProductionTeamHandler(IProductionTeamRepository repo, IUnitOfWork uow, IValidator<CreateProductionTeamCommand> validator)
    : ICommandHandler<CreateProductionTeamCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateProductionTeamCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var team = ProductionTeam.Create(
                cmd.Code, cmd.Name, cmd.OrgUnitId,
                cmd.StandardLaborQuantity, cmd.ProductionRate,
                cmd.IsOrderBasedPlanningEnabled,
                cmd.ProductGroupCategoryIds,
                cmd.CreatedBy);
            await repo.AddAsync(team, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(team.TeamCode);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<string>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
