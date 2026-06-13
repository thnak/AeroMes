using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.StageHandovers.Commands.CreateHandoverForm;

public sealed class CreateHandoverFormHandler(
    IStageHandoverRepository repo,
    IValidator<CreateHandoverFormCommand> validator,
    IUnitOfWork uow) : ICommandHandler<CreateHandoverFormCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateHandoverFormCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<int>.Invalid(vr.ToErrorDictionary());

        try
        {
            var formNumber = await repo.GenerateFormNumberAsync(cmd.FormType, ct);
            var form = StageHandoverForm.Create(
                formNumber, cmd.FormType,
                cmd.FromWorkOrderId, cmd.FromRoutingStepId,
                cmd.ToWorkOrderId, cmd.ToRoutingStepId,
                cmd.HandoverDate, cmd.Notes, cmd.CreatedBy);

            foreach (var line in cmd.Lines)
                form.AddLine(line.ProductCode, line.Qty, line.Unit, line.Notes);

            form.Submit(cmd.CreatedBy);
            await repo.AddAsync(form, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(form.FormID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
