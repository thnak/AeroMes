using AeroMes.Application.Common;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.DuplicateProcessStep;

public class DuplicateProcessStepHandler(IProductionProcessStepRepository repository)
    : ICommandHandler<DuplicateProcessStepCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        DuplicateProcessStepCommand command, CancellationToken ct)
    {
        var source = await repository.GetByIdAsync(command.StepID, ct);
        if (source is null) return ValidationResult<int>.NotFound($"Công đoạn #{command.StepID} không tồn tại.");

        if (await repository.CodeExistsAsync(command.NewCode, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["newCode"] = ["Mã công đoạn đã tồn tại."]
            });

        var copy = ProductionProcessStep.Create(
            command.NewCode, source.Name, source.Description,
            source.ApplicationScope, source.ProductGroupIdsJson,
            source.ProductIdsJson, command.CreatedBy);

        var id = await repository.AddAsync(copy, ct);
        return ValidationResult<int>.Ok(id);
    }
}
