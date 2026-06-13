using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Commands.CreateStandardSet;

public class CreateStandardSetHandler(
    IQualityStandardSetRepository repository,
    IProductionProcessRepository processRepository,
    IValidator<CreateStandardSetCommand> validator)
    : ICommandHandler<CreateStandardSetCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateStandardSetCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repository.CodeExistsAsync(command.Code, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["code"] = ["Mã bộ tiêu chuẩn đã tồn tại."]
            });

        try
        {
            var processId = await processRepository.GetActiveProcessIdForProductAsync(command.ProductCode, ct);

            var standardSet = QualityStandardSet.Create(
                command.Code, command.Name, command.ProductCode,
                command.SamplingMethodID, command.EffectiveDate,
                command.Notes, processId, command.CreatedBy);

            var id = await repository.AddAsync(standardSet, ct);

            if (command.ProductCriteria.Count > 0 || command.StageCriteria.Count > 0)
            {
                var entity = await repository.GetByIdAsync(id, ct);
                if (entity is not null)
                {
                    entity.SetProductCriteria(
                        command.ProductCriteria.Select(c => (c.CriteriaId, c.Parameters)),
                        command.CreatedBy);
                    entity.SetStageCriteria(
                        command.StageCriteria.Select(c => (c.StageId, c.CriteriaId, c.SamplingMethodId, c.Parameters)),
                        command.CreatedBy);
                    await repository.SaveChangesAsync(ct);
                }
            }

            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
