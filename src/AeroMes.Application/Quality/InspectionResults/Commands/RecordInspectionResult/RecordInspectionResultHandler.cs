using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionResults.Commands.RecordInspectionResult;

public class RecordInspectionResultHandler(
    IInspectionOrderRepository orderRepo,
    IInspectionPlanRepository planRepo,
    IInspectionResultRepository resultRepo,
    IUnitOfWork uow,
    IValidator<RecordInspectionResultCommand> validator)
    : ICommandHandler<RecordInspectionResultCommand, ValidationResult<InspectionResultDto>>
{
    public async Task<ValidationResult<InspectionResultDto>> HandleAsync(
        RecordInspectionResultCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<InspectionResultDto>.Invalid(validation.ToErrorDictionary());

        try
        {
            var order = await orderRepo.GetByIdAsync(cmd.InspectionOrderId, ct);
            if (order is null)
                return ValidationResult<InspectionResultDto>.NotFound(
                    $"InspectionOrder '{cmd.InspectionOrderId}' was not found.");

            if (order.Status != "IN_PROGRESS")
                return ValidationResult<InspectionResultDto>.Failure(
                    $"Cannot record results for order in status '{order.Status}'. Order must be IN_PROGRESS.");

            var plan = await planRepo.GetByIdWithCharacteristicsAsync(order.PlanId, ct);
            if (plan is null)
                return ValidationResult<InspectionResultDto>.NotFound(
                    $"InspectionPlan '{order.PlanId}' was not found.");

            var characteristic = plan.Characteristics.FirstOrDefault(c => c.CharId == cmd.CharId);
            if (characteristic is null)
                return ValidationResult<InspectionResultDto>.Failure(
                    $"Characteristic '{cmd.CharId}' does not belong to inspection plan '{plan.PlanId}'.");

            var isWithinSpec = InspectionEvaluator.EvaluateWithinSpec(
                characteristic, cmd.MeasuredValue, cmd.AttributeResult);

            var result = InspectionResult.Create(
                cmd.InspectionOrderId, cmd.CharId,
                cmd.MeasuredValue, cmd.AttributeResult, isWithinSpec,
                cmd.DefectCodeId, cmd.SampleIndex, cmd.Notes, cmd.RecordedBy);

            resultRepo.Add(result);

            // Evaluate outcome
            if (!isWithinSpec && characteristic.SeverityLevel == "CRITICAL")
            {
                order.Fail();
            }
            else
            {
                var existingResults = await resultRepo.GetByOrderAsync(cmd.InspectionOrderId, ct);
                List<InspectionResult> allResults = [.. existingResults, result];
                var requiredChars = plan.Characteristics.Where(c => c.IsRequired).ToList();

                var outcome = InspectionEvaluator.EvaluateOrderOutcome(
                    plan, requiredChars, allResults, null);

                if (outcome == "PASSED") order.Pass();
                else if (outcome == "FAILED") order.Fail();
            }

            await uow.SaveChangesAsync(ct);

            var dto = new InspectionResultDto(
                result.ResultId, result.InspectionOrderId, result.CharId,
                characteristic.CharName, characteristic.MeasurementType,
                result.MeasuredValue, result.AttributeResult, result.IsWithinSpec,
                null, result.SampleIndex, result.Notes, result.RecordedBy, result.RecordedAt);

            return ValidationResult<InspectionResultDto>.Ok(dto);
        }
        catch (DomainException ex)
        {
            return ValidationResult<InspectionResultDto>.Failure(ex.Message);
        }
    }
}
