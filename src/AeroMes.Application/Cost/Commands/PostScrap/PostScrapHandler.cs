using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.Commands.PostScrap;

public class PostScrapHandler(
    IScrapTransactionRepository repository,
    IUnitOfWork uow,
    IValidator<PostScrapCommand> validator)
    : ICommandHandler<PostScrapCommand, ValidationResult<long>>
{
    public async Task<ValidationResult<long>> HandleAsync(PostScrapCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<long>.Invalid(v.ToErrorDictionary());

        try
        {
            var tx = ScrapTransaction.Create(
                command.WOID, command.LogID, command.DefectCodeId,
                command.ProductCode, command.LotNumber, command.ScrapQty,
                command.MaterialCostPerUnit, command.LaborCostSunk, command.MachineCostSunk,
                command.DisposalMethod, command.ScrapLocationId,
                command.ApprovedBy, command.Notes, command.CreatedBy);

            await repository.AddAsync(tx, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<long>.Ok(tx.ScrapTxID);
        }
        catch (DomainException ex) { return ValidationResult<long>.Failure(ex.Message); }
    }
}
