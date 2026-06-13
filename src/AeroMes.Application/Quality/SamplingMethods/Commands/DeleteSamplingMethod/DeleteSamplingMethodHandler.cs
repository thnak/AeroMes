using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.SamplingMethods.Commands.DeleteSamplingMethod;

public class DeleteSamplingMethodHandler(ISamplingMethodRepository repo)
    : ICommandHandler<DeleteSamplingMethodCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteSamplingMethodCommand cmd, CancellationToken ct)
    {
        var method = await repo.GetByIdAsync(cmd.SamplingMethodID, ct);
        if (method is null)
            return ValidationResult<Unit>.NotFound($"Phương pháp lấy mẫu '{cmd.SamplingMethodID}' không tồn tại.");

        method.SoftDelete();
        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
