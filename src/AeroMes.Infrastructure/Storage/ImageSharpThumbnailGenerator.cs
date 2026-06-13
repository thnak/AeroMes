using AeroMes.Application.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AeroMes.Infrastructure.Storage;

public sealed class ImageSharpThumbnailGenerator : IThumbnailGenerator
{
    private static readonly HashSet<string> SupportedTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/gif"
    ];

    public bool CanProcess(string contentType) =>
        SupportedTypes.Contains(contentType.ToLowerInvariant());

    public async Task<Stream?> GenerateAsync(
        Stream source, string contentType, int maxDimension = 256, CancellationToken ct = default)
    {
        try
        {
            using var img = await Image.LoadAsync(source, ct);
            int w = img.Width, h = img.Height;
            if (w > maxDimension || h > maxDimension)
            {
                double scale = Math.Min((double)maxDimension / w, (double)maxDimension / h);
                img.Mutate(x => x.Resize((int)(w * scale), (int)(h * scale)));
            }

            var ms = new MemoryStream();
            await img.SaveAsJpegAsync(ms, ct);
            ms.Position = 0;
            return ms;
        }
        catch
        {
            return null;
        }
    }
}
