using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.ProductionTeams.Commands.AddTeamMember;

public class AddTeamMemberHandler(IProductionTeamRepository repo, IUnitOfWork uow, IValidator<AddTeamMemberCommand> validator)
    : ICommandHandler<AddTeamMemberCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(AddTeamMemberCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        try
        {
            var team = await repo.GetByCodeAsync(cmd.TeamCode, ct);
            if (team is null) return ValidationResult<int>.NotFound($"Entity '{cmd.TeamCode}' was not found.");

            var member = team.AddMember(cmd.EmployeeCode, cmd.IsLeader, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(member.MemberId);
        }        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
