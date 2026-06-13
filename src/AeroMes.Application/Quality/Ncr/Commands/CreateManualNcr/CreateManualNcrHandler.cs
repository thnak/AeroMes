using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Commands.CreateManualNcr;

public class CreateManualNcrHandler(
    INcrRepository ncrRepo,
    IUnitOfWork uow,
    IValidator<CreateManualNcrCommand> validator)
    : ICommandHandler<CreateManualNcrCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateManualNcrCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<int>.Invalid(validation.ToErrorDictionary());

        var ncrNo = $"NCR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";

        var ncr = Domain.Quality.Ncr.Create(
            ncrNo,
            null,
            cmd.WorkOrderId,
            cmd.ProductCode,
            cmd.LotNumber,
            cmd.QtyAffected,
            cmd.Severity,
            cmd.CreatedBy);

        ncrRepo.Add(ncr);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(ncr.NcrId);
    }
}
