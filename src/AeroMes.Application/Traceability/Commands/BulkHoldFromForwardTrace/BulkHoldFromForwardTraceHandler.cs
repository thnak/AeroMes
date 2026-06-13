using AeroMes.Application.Common;
using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using LiteBus.Events.Abstractions;
using AeroMes.Domain.Traceability.Events;

namespace AeroMes.Application.Traceability.Commands.BulkHoldFromForwardTrace;

public sealed class BulkHoldFromForwardTraceHandler(
    ILotHoldRepository holdRepository,
    ILotTraceabilityRepository traceRepository,
    IEventMediator eventMediator,
    IValidator<BulkHoldFromForwardTraceCommand> validator)
    : ICommandHandler<BulkHoldFromForwardTraceCommand, ValidationResult<BulkHoldResultDto>>
{
    public async Task<ValidationResult<BulkHoldResultDto>> HandleAsync(
        BulkHoldFromForwardTraceCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<BulkHoldResultDto>.Invalid(vr.ToErrorDictionary());

        try
        {
            // Run forward trace to get all downstream lots
            var genealogy = await traceRepository.ForwardTraceAsync(
                command.SuspectLotNumber, command.MaxDepth, ct);

            var downstreamLots = genealogy.Nodes
                .Select(n => n.LotNumber)
                .Where(l => !string.Equals(l, command.SuspectLotNumber, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();

            // Also place hold on the suspect lot itself
            var allLots = new[] { command.SuspectLotNumber }.Concat(downstreamLots).Distinct().ToList();

            var holds = allLots.Select(lot => LotHold.Place(
                lot, command.HoldReason, command.InitiatedBy,
                holdDescription: command.HoldDescription,
                holdReference: command.HoldReference)).ToList();

            foreach (var hold in holds)
                await holdRepository.AddAsync(hold, ct);

            await holdRepository.SaveChangesAsync(ct);

            var affectedLots = holds.Select(h => h.LotNumber).ToList();
            await eventMediator.PublishAsync(
                new BulkHoldAppliedEvent(
                    command.SuspectLotNumber.ToUpperInvariant(),
                    affectedLots.Count,
                    affectedLots), null, ct);

            return ValidationResult<BulkHoldResultDto>.Ok(new BulkHoldResultDto(
                allLots.Count, holds.Count, affectedLots));
        }
        catch (DomainException ex)
        {
            return ValidationResult<BulkHoldResultDto>.Failure(ex.Message);
        }
    }
}
