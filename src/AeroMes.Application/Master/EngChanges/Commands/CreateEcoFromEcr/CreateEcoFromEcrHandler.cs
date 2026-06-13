using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.CreateEcoFromEcr;

public class CreateEcoFromEcrHandler(
    IEngChangeRepository repo,
    IUnitOfWork uow,
    IValidator<CreateEcoFromEcrCommand> validator) : ICommandHandler<CreateEcoFromEcrCommand, ValidationResult<string>>
{
    public async Task<ValidationResult<string>> HandleAsync(CreateEcoFromEcrCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<string>.Invalid(validation.ToErrorDictionary());

        try
        {
            var ecr = await repo.GetByNumberAsync(cmd.EcrNumber, ct)
                ?? throw new EntityNotFoundException(nameof(EngChange), cmd.EcrNumber);

            if (ecr.EcType != EcType.Ecr)
                throw new DomainException($"Phiếu '{ecr.EcNumber}' không phải là ECR.");
            if (ecr.Status != EcStatus.Approved)
                throw new DomainException(
                    $"Chỉ ECR đã được phê duyệt mới được chuyển thành ECO. Trạng thái hiện tại: {ecr.Status}.");

            var eco = EngChange.Create(
                cmd.NewEcNumber, EcType.Eco, ecr.Title, ecr.Description,
                ecr.Reason, ecr.Priority, ecr.TargetDate,
                ecr.AffectedProducts, ecr.EcNumber, cmd.RequestedBy);
            await repo.AddAsync(eco, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<string>.Ok(eco.EcNumber);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<string>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<string>.Failure(ex.Message);
        }
    }
}
