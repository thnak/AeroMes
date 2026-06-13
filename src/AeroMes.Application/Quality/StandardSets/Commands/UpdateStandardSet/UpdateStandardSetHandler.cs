using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Commands.UpdateStandardSet;

public class UpdateStandardSetHandler(IQualityStandardSetRepository repository)
    : ICommandHandler<UpdateStandardSetCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateStandardSetCommand command, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(command.StandardSetID, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound("Bộ tiêu chuẩn không tồn tại.");

        try
        {
            entity.Update(command.Name, command.SamplingMethodID, command.EffectiveDate, command.Notes, command.UpdatedBy);
            entity.SetProductCriteria(
                command.ProductCriteria.Select(c => (c.CriteriaId, c.Parameters)),
                command.UpdatedBy);
            entity.SetStageCriteria(
                command.StageCriteria.Select(c => (c.StageId, c.CriteriaId, c.SamplingMethodId, c.Parameters)),
                command.UpdatedBy);
            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
