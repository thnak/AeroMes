using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionResults.Commands.BulkRecordInspectionResults;

public class BulkRecordInspectionResultsHandler(
    IInspectionOrderRepository orderRepo,
    IInspectionPlanRepository planRepo,
    IInspectionResultRepository resultRepo,
    IUnitOfWork uow,
    IValidator<BulkRecordInspectionResultsCommand> validator)
    : ICommandHandler<BulkRecordInspectionResultsCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        BulkRecordInspectionResultsCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var order = await orderRepo.GetByIdAsync(cmd.InspectionOrderId, ct);
            if (order is null)
                return ValidationResult<Unit>.NotFound(
                    $"InspectionOrder '{cmd.InspectionOrderId}' was not found.");

            if (order.Status != "IN_PROGRESS")
                return ValidationResult<Unit>.Failure(
                    $"Cannot record results for order in status '{order.Status}'. Order must be IN_PROGRESS.");

            var plan = await planRepo.GetByIdWithCharacteristicsAsync(order.PlanId, ct);
            if (plan is null)
                return ValidationResult<Unit>.NotFound(
                    $"InspectionPlan '{order.PlanId}' was not found.");

            var charMap = plan.Characteristics.ToDictionary(c => c.CharId);
            var newResults = new List<InspectionResult>();
            InspectionCharacteristic? criticalFailedChar = null;

            foreach (var item in cmd.Results)
            {
                if (!charMap.TryGetValue(item.CharId, out var characteristic))
                    return ValidationResult<Unit>.Failure(
                        $"Characteristic '{item.CharId}' does not belong to inspection plan '{plan.PlanId}'.");

                var isWithinSpec = InspectionEvaluator.EvaluateWithinSpec(
                    characteristic, item.MeasuredValue, item.AttributeResult);

                var result = InspectionResult.Create(
                    cmd.InspectionOrderId, item.CharId,
                    item.MeasuredValue, item.AttributeResult, isWithinSpec,
                    item.DefectCodeId, item.SampleIndex, item.Notes, cmd.RecordedBy);

                newResults.Add(result);

                if (!isWithinSpec && characteristic.SeverityLevel == "CRITICAL")
                    criticalFailedChar = characteristic;
            }

            resultRepo.AddRange(newResults);

            // Evaluate outcome once after all items processed
            if (criticalFailedChar is not null)
            {
                order.Fail();
            }
            else
            {
                var existingResults = await resultRepo.GetByOrderAsync(cmd.InspectionOrderId, ct);
                List<InspectionResult> allResults = [.. existingResults, .. newResults];
                var requiredChars = plan.Characteristics.Where(c => c.IsRequired).ToList();

                var outcome = InspectionEvaluator.EvaluateOrderOutcome(
                    plan, requiredChars, allResults, null);

                if (outcome == "PASSED") order.Pass();
                else if (outcome == "FAILED") order.Fail();
            }

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
