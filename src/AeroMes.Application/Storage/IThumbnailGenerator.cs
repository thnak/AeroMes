namespace AeroMes.Application.Storage;

public interface IThumbnailGenerator
{
    bool CanProcess(string contentType);
    Task<Stream?> GenerateAsync(Stream source, string contentType, int maxDimension = 256, CancellationToken ct = default);
}
