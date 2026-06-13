using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Commands.CreateProductionProcess;

public class CreateProductionProcessHandler(
    IProductionProcessRepository repository,
    IValidator<CreateProductionProcessCommand> validator)
    : ICommandHandler<CreateProductionProcessCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateProductionProcessCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repository.CodeExistsAsync(command.Code, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["code"] = ["Mã quy trình đã tồn tại."]
            });

        try
        {
            var process = ProductionProcess.Create(
                command.Code, command.Name, command.ProcessType, command.EffectiveDate,
                command.ApplicationScope, command.ProductGroupIdsJson, command.ProductIdsJson,
                command.IsForPlanningOnly, command.CreatedBy);

            var id = await repository.AddAsync(process, ct);

            foreach (var s in command.Stages)
                process.AddStage(ProductionProcessStage.Create(
                    id, s.SortOrder, s.ProcessStepCode, s.CapacityType,
                    s.CapacityIdsJson, s.PlannedTimeSeconds, s.PlannedTimeSource,
                    s.TimeOffsetDays, s.IsPrimaryStage));

            await repository.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
