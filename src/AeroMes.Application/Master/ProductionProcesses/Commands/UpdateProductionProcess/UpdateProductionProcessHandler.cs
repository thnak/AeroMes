using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Commands.UpdateProductionProcess;

public class UpdateProductionProcessHandler(IProductionProcessRepository repository)
    : ICommandHandler<UpdateProductionProcessCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateProductionProcessCommand command, CancellationToken ct)
    {
        var process = await repository.GetByIdAsync(command.ProcessID, ct);
        if (process is null) return ValidationResult<Unit>.NotFound($"Quy trình #{command.ProcessID} không tồn tại.");

        try
        {
            process.Update(command.Name, command.EffectiveDate, command.ApplicationScope,
                command.ProductGroupIdsJson, command.ProductIdsJson,
                command.IsForPlanningOnly, command.UpdatedBy);
            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
