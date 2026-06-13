using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.DuplicateProductionTeam;

public class DuplicateProductionTeamHandler(IProductionTeamRepository repo, IUnitOfWork uow, IValidator<DuplicateProductionTeamCommand> validator)
    : ICommandHandler<DuplicateProductionTeamCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(DuplicateProductionTeamCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var source = await repo.GetByCodeAsync(cmd.SourceCode, ct)
                ?? throw new EntityNotFoundException(nameof(ProductionTeam), cmd.SourceCode);

            var copy = source.Duplicate(cmd.NewCode, cmd.CreatedBy);
            await repo.AddAsync(copy, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(copy.TeamCode);
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
