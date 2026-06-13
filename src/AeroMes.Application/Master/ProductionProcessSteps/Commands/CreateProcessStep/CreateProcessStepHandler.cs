using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcessSteps.Commands.CreateProcessStep;

public class CreateProcessStepHandler(
    IProductionProcessStepRepository repository,
    IValidator<CreateProcessStepCommand> validator)
    : ICommandHandler<CreateProcessStepCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateProcessStepCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repository.CodeExistsAsync(command.Code, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["code"] = ["Mã công đoạn đã tồn tại."]
            });

        try
        {
            var step = ProductionProcessStep.Create(
                command.Code, command.Name, command.Description,
                command.ApplicationScope, command.ProductGroupIdsJson,
                command.ProductIdsJson, command.CreatedBy);

            var id = await repository.AddAsync(step, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
