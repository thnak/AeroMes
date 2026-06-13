using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/qr")]
[Authorize]
public sealed class QrController : ControllerBase
{
    /// <summary>
    /// Generate a QR code PNG for any entity by type + id.
    /// The encoded payload is a URL: {baseUrl}/{entityType}/{id}
    /// </summary>
    [HttpGet("{entityType}/{id}")]
    [Produces("image/png")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = ["size"])]
    public IActionResult GetQrCode(
        string entityType,
        string id,
        [FromQuery] int size = 10,
        [FromQuery] string? baseUrl = null)
    {
        if (size is < 1 or > 40) size = 10;

        var origin = baseUrl ?? $"{Request.Scheme}://{Request.Host}";
        var payload = $"{origin}/{Uri.EscapeDataString(entityType)}/{Uri.EscapeDataString(id)}";

        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(size);

        return File(pngBytes, "image/png");
    }

    /// <summary>
    /// Generate a QR code PNG for an arbitrary text payload (max 2000 chars).
    /// </summary>
    [HttpPost("text")]
    [Produces("image/png")]
    public IActionResult GetQrCodeFromText([FromBody] QrTextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 2000)
            return BadRequest("Text must be 1–2000 characters.");

        var size = request.Size is >= 1 and <= 40 ? request.Size : 10;

        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(request.Text, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(size);

        return File(pngBytes, "image/png");
    }
}

public sealed record QrTextRequest(string Text, int Size = 10);
