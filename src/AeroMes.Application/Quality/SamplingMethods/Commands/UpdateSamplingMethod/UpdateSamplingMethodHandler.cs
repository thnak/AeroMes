using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.SamplingMethods.Commands.UpdateSamplingMethod;

public class UpdateSamplingMethodHandler(ISamplingMethodRepository repo)
    : ICommandHandler<UpdateSamplingMethodCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateSamplingMethodCommand cmd, CancellationToken ct)
    {
        var method = await repo.GetByIdAsync(cmd.SamplingMethodID, ct);
        if (method is null)
            return ValidationResult<Unit>.NotFound($"Phương pháp lấy mẫu '{cmd.SamplingMethodID}' không tồn tại.");

        method.UpdateDetails(cmd.Name, cmd.SamplingType, cmd.SampleQuantity, cmd.MaxDefects, cmd.Notes, cmd.Status, null);

        method.ClearVolumeRanges(null);
        if (cmd.VolumeRanges is { Count: > 0 })
            foreach (var r in cmd.VolumeRanges)
                method.AddVolumeRange(r.MinQty, r.MaxQty, r.SampleSizeOrRatio, r.MaxDefects, null);

        await repo.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
