using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.SamplingMethods.Commands.CreateSamplingMethod;

public class CreateSamplingMethodHandler(
    ISamplingMethodRepository repo,
    IValidator<CreateSamplingMethodCommand> validator)
    : ICommandHandler<CreateSamplingMethodCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateSamplingMethodCommand cmd, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repo.CodeExistsAsync(cmd.Code, ct))
            return ValidationResult<int>.Failure($"Mã phương pháp '{cmd.Code}' đã tồn tại.");

        var method = SamplingMethod.Create(cmd.Code, cmd.Name, cmd.SamplingType,
            cmd.SampleQuantity, cmd.MaxDefects, cmd.Notes, null);

        if (cmd.VolumeRanges is { Count: > 0 })
            foreach (var r in cmd.VolumeRanges)
                method.AddVolumeRange(r.MinQty, r.MaxQty, r.SampleSizeOrRatio, r.MaxDefects, null);

        await repo.AddAsync(method, ct);
        await repo.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(method.SamplingMethodID);
    }
}
