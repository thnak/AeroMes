using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.CreateStockPolicy;

public class CreateStockPolicyHandler(
    IStockPolicyRepository repo,
    IUnitOfWork uow,
    IValidator<CreateStockPolicyCommand> validator)
    : ICommandHandler<CreateStockPolicyCommand, ValidationResult<StockPolicyCreatedResult>>
{
    public async Task<ValidationResult<StockPolicyCreatedResult>> HandleAsync(
        CreateStockPolicyCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<StockPolicyCreatedResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            if (await repo.ExistsForProductAndLocationAsync(cmd.ProductCode, cmd.LocationId, null, ct))
                return ValidationResult<StockPolicyCreatedResult>.Failure(
                    $"Đã tồn tại chính sách tồn kho cho sản phẩm '{cmd.ProductCode}' tại vị trí #{cmd.LocationId}.");

            var policy = StockPolicy.Create(
                cmd.ProductCode, cmd.LocationId,
                cmd.MinQty, cmd.MaxQty, cmd.SafetyStockQty,
                cmd.ReorderQty, cmd.LeadTimeDays, cmd.CreatedBy);

            await repo.AddAsync(policy, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<StockPolicyCreatedResult>.Ok(new(policy.PolicyId));
        }
        catch (DomainException ex)
        {
            return ValidationResult<StockPolicyCreatedResult>.Failure(ex.Message);
        }
    }
}
