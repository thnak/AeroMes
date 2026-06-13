using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Commands.CreateCriteria;

public class CreateCriteriaHandler(
    IQualityCriteriaRepository repository,
    IValidator<CreateCriteriaCommand> validator)
    : ICommandHandler<CreateCriteriaCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateCriteriaCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repository.CodeExistsAsync(command.Code, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["code"] = ["Mã tiêu chí đã tồn tại."]
            });

        try
        {
            var criteria = QualityCriteria.Create(
                command.Code, command.Name, command.GroupID,
                command.CriteriaType, command.InspectionMethod,
                command.MethodDescription, command.CreatedBy);

            var id = await repository.AddAsync(criteria, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
