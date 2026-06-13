using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ReceiveFinishedProductIntakeRequest;

public class ReceiveFinishedProductIntakeRequestHandler(
    IFinishedProductIntakeRequestRepository intakeRepo,
    IUnitOfWork uow,
    IValidator<ReceiveFinishedProductIntakeRequestCommand> validator)
    : ICommandHandler<ReceiveFinishedProductIntakeRequestCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        ReceiveFinishedProductIntakeRequestCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var request = await intakeRepo.GetByIdWithLinesAsync(cmd.IntakeRequestId, ct);
        if (request is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu nhập '{cmd.IntakeRequestId}' không tồn tại.");

        var knownLineIds = request.Lines.Select(l => l.LineId).ToHashSet();
        foreach (var input in cmd.ReceiptLines)
        {
            if (!knownLineIds.Contains(input.LineId))
                return ValidationResult<Unit>.NotFound(
                    $"Dòng '{input.LineId}' không thuộc yêu cầu nhập này.");
        }

        try
        {
            var actualQtyByLineId = cmd.ReceiptLines.ToDictionary(l => l.LineId, l => l.ActualReceivedQuantity);
            request.Receive(actualQtyByLineId, cmd.ReceivedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
