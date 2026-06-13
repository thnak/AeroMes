using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.ApplyRecallQuarantine;

public sealed class ApplyRecallQuarantineHandler(
    IRecallRepository recallRepository,
    ILotHoldRepository holdRepository,
    IValidator<ApplyRecallQuarantineCommand> validator)
    : ICommandHandler<ApplyRecallQuarantineCommand, ValidationResult<RecallQuarantineResultDto>>
{
    public async Task<ValidationResult<RecallQuarantineResultDto>> HandleAsync(
        ApplyRecallQuarantineCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<RecallQuarantineResultDto>.Invalid(vr.ToErrorDictionary());

        var recall = await recallRepository.GetByIdAsync(command.RecallID, ct);
        if (recall is null) return ValidationResult<RecallQuarantineResultDto>.NotFound(
            $"Recall {command.RecallID} not found.");

        try
        {
            var scopeLots = await recallRepository.GetScopeLotsAsync(command.RecallID, ct);

            // Hold all lots that are NOT shipped (shipped lots require different workflow)
            var lotsToHold = scopeLots
                .Where(l => l.LotCategory != LotCategory.Shipped)
                .ToList();

            var affectedLots = new List<string>();
            foreach (var sl in lotsToHold)
            {
                var hold = LotHold.Place(
                    sl.LotNumber,
                    HoldReason.RecallInvestigation,
                    command.AppliedBy,
                    sl.ProductCode,
                    holdDescription: $"Quarantined by recall {recall.RecallCode}",
                    holdReference: recall.RecallCode);

                await holdRepository.AddAsync(hold, ct);
                sl.LinkHold(hold.HoldID);
                affectedLots.Add(sl.LotNumber);
            }

            recall.MarkQuarantineApplied(lotsToHold.Count, affectedLots);

            await recallRepository.AddAuditEntryAsync(
                RecallAuditEntry.Log(recall.RecallID, "QuarantineApplied", command.AppliedBy,
                    $"Placed {lotsToHold.Count} holds for recall {recall.RecallCode}",
                    systemGenerated: false),
                ct);

            await recallRepository.SaveChangesAsync(ct);

            return ValidationResult<RecallQuarantineResultDto>.Ok(
                new RecallQuarantineResultDto(command.RecallID, lotsToHold.Count, affectedLots));
        }
        catch (DomainException ex)
        {
            return ValidationResult<RecallQuarantineResultDto>.Failure(ex.Message);
        }
    }
}
