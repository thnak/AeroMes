using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.RecordCycleCountLine;

public class RecordCycleCountLineHandler(
    ICycleCountPlanRepository repo,
    IUnitOfWork uow,
    IValidator<RecordCycleCountLineCommand> validator)
    : ICommandHandler<RecordCycleCountLineCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        RecordCycleCountLineCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var plan = await repo.GetByIdWithLinesAsync(cmd.PlanId, ct);
            if (plan is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy kế hoạch kiểm kê #{cmd.PlanId}.");

            plan.RecordCount(cmd.LineId, cmd.CountedQty, cmd.CountedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
