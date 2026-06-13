using AeroMes.Application.Labeling.Services;
using AeroMes.Domain.Labeling;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Labeling.Queries.GenerateLabel;

public sealed class GenerateLabelHandler(
    IIdentityEncodingService encoder,
    ILabelRenderer renderer,
    IProductRepository products,
    IStorageLocationRepository locations,
    IWorkCenterRepository workCenters) : IQueryHandler<GenerateLabelQuery, GenerateLabelResult>
{
    public async Task<GenerateLabelResult> HandleAsync(GenerateLabelQuery query, CancellationToken ct)
    {
        var format    = query.Format.ToUpperInvariant();
        var output    = query.Output.ToLowerInvariant();
        var contentType = query.ContentType.ToUpperInvariant();

        var (qrPayload, fields) = await BuildPayloadAsync(contentType, query.EntityId, ct);

        var rendered = format == "COMPOSITE"
            ? renderer.RenderComposite(qrPayload, fields, output)
            : renderer.RenderCompact(qrPayload, fields, output);

        var ext = output switch
        {
            "zpl" => "zpl",
            "pdf" => "pdf",
            _     => "png",
        };

        return new GenerateLabelResult(rendered.Data, rendered.ContentType, $"label_{contentType}_{query.EntityId}.{ext}");
    }

    private async Task<(string, IReadOnlyDictionary<string, string>)> BuildPayloadAsync(
        string contentType, string entityId, CancellationToken ct)
    {
        return contentType switch
        {
            LabelContentType.Product     => await BuildProductAsync(entityId, ct),
            LabelContentType.Location    => await BuildLocationAsync(entityId, ct),
            LabelContentType.Workstation => await BuildWorkstationAsync(entityId, ct),
            _                            => BuildGeneric(contentType, entityId),
        };
    }

    private async Task<(string, IReadOnlyDictionary<string, string>)> BuildProductAsync(string code, CancellationToken ct)
    {
        var product = await products.GetByCodeAsync(code, ct);
        var fields = new Dictionary<string, string>
        {
            ["Type"] = "Product",
            ["Code"] = code,
            ["Name"] = product?.ProductName ?? code,
            ["UOM"]  = product?.BaseUoMCode ?? "",
        };
        return (encoder.EncodeProduct(code, null, null), fields);
    }

    private async Task<(string, IReadOnlyDictionary<string, string>)> BuildLocationAsync(string entityId, CancellationToken ct)
    {
        var fields = new Dictionary<string, string> { ["Type"] = "Location", ["ID"] = entityId };

        if (int.TryParse(entityId, out var locId))
        {
            var loc = await locations.GetByIdAsync(locId, ct);
            if (loc != null)
            {
                fields["Code"] = loc.LocationCode;
                fields["Name"] = loc.LocationName;
            }
        }

        return (encoder.EncodeLocation(entityId, null, null), fields);
    }

    private async Task<(string, IReadOnlyDictionary<string, string>)> BuildWorkstationAsync(string entityId, CancellationToken ct)
    {
        var fields = new Dictionary<string, string> { ["Type"] = "Workstation", ["ID"] = entityId };

        if (int.TryParse(entityId, out var wcId))
        {
            var wc = await workCenters.GetByIdAsync(wcId, ct);
            if (wc != null) fields["Name"] = wc.WorkCenterName;
        }

        return (encoder.EncodeWorkstation(entityId, null), fields);
    }

    private (string, IReadOnlyDictionary<string, string>) BuildGeneric(string contentType, string entityId)
    {
        var fields = new Dictionary<string, string>
        {
            ["Type"] = contentType,
            ["ID"]   = entityId,
        };
        return (encoder.EncodeMaterial(entityId, null, null), fields);
    }
}
