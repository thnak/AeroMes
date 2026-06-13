using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MRP.Commands.CreateMrp;

public class CreateMrpHandler(IMaterialRequirementsPlanRepository repository)
    : ICommandHandler<CreateMrpCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateMrpCommand command, CancellationToken ct)
    {
        if (await repository.PlanNumberExistsAsync(command.PlanNumber, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["planNumber"] = ["Số kế hoạch đã tồn tại."]
            });

        try
        {
            var plan = MaterialRequirementsPlan.Create(
                command.PlanNumber, command.PlanName, command.MasterPlanId,
                command.OrganizationalUnit, command.PeriodStart, command.PeriodEnd,
                command.Notes, command.CreatedBy);

            var id = await repository.AddAsync(plan, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
