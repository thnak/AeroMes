using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Statistics.Commands.SubmitProductionStatisticsSheet;

public sealed class SubmitProductionStatisticsSheetHandler(
    IProductionStatisticsSheetRepository repo,
    IUnitOfWork uow) : ICommandHandler<SubmitProductionStatisticsSheetCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SubmitProductionStatisticsSheetCommand cmd, CancellationToken ct)
    {
        var sheet = await repo.GetByIdAsync(cmd.SheetId, ct);
        if (sheet is null)
            return ValidationResult<Unit>.NotFound($"Statistics sheet #{cmd.SheetId} not found.");

        try
        {
            sheet.Submit();
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
