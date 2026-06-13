using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Commands.UpdateCriteria;

public class UpdateCriteriaHandler(IQualityCriteriaRepository repository)
    : ICommandHandler<UpdateCriteriaCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateCriteriaCommand command, CancellationToken ct)
    {
        var criteria = await repository.GetByIdAsync(command.CriteriaID, ct);
        if (criteria is null) return ValidationResult<Unit>.NotFound($"Tiêu chí #{command.CriteriaID} không tồn tại.");

        try
        {
            criteria.Update(command.Name, command.GroupID, command.CriteriaType,
                command.InspectionMethod, command.MethodDescription, command.UpdatedBy);
            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
