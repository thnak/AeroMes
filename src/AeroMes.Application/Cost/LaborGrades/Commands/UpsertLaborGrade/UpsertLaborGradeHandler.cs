using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.LaborGrades.Commands.UpsertLaborGrade;

public class UpsertLaborGradeHandler(
    ILaborGradeRepository repository,
    IValidator<UpsertLaborGradeCommand> validator)
    : ICommandHandler<UpsertLaborGradeCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        UpsertLaborGradeCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        try
        {
            var existing = await repository.GetActiveByCodeAsync(command.GradeCode, ct);
            if (existing is not null && existing.EffectiveFrom >= command.EffectiveFrom)
                return ValidationResult<int>.Failure("Ngày hiệu lực mới phải sau ngày hiệu lực hiện tại.");

            existing?.ExpireOn(command.EffectiveFrom.AddDays(-1));

            var grade = LaborGrade.Create(
                command.GradeCode, command.GradeName, command.HourlyRate,
                command.OvertimeMultiplier, command.EffectiveFrom, command.Currency, command.CreatedBy);

            var id = await repository.AddAsync(grade, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
