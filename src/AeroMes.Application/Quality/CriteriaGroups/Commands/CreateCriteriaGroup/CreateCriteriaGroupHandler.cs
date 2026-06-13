using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Commands.CreateCriteriaGroup;

public class CreateCriteriaGroupHandler(
    IQualityCriteriaGroupRepository repository,
    IValidator<CreateCriteriaGroupCommand> validator)
    : ICommandHandler<CreateCriteriaGroupCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateCriteriaGroupCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var normalizedCode = command.Code.Trim().ToUpperInvariant();
        if (await repository.CodeExistsAsync(normalizedCode, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["code"] = ["Mã nhóm đã tồn tại."]
            });

        try
        {
            var group = QualityCriteriaGroup.Create(command.Code, command.Name, command.CreatedBy);
            var id = await repository.AddAsync(group, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
