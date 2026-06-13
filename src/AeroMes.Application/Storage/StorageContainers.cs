namespace AeroMes.Application.Storage;

/// <summary>
/// Logical storage containers (top-level key prefixes). Each document domain gets its
/// own container so retention, access rules, and clean-up can be reasoned about per area.
/// Values are lowercase, URL/path-safe segments.
/// </summary>
public static class StorageContainers
{
    /// <summary>Standard Operating Procedure documents &amp; work instructions (e.g. SopDocument).</summary>
    public const string Sop = "sop";

    /// <summary>Production / product specification documents (e.g. ProductSpecification, drawings).</summary>
    public const string ProductionSpec = "production-spec";

    /// <summary>Product images &amp; thumbnails.</summary>
    public const string ProductImage = "product-image";

    /// <summary>Quality defect photos and evidence.</summary>
    public const string QualityPhoto = "quality-photo";

    /// <summary>Laboratory Certificates of Analysis (CoA) and test attachments.</summary>
    public const string LabCoa = "lab-coa";

    /// <summary>Generated label / print artifacts (e.g. composite label PDFs).</summary>
    public const string Label = "label";

    /// <summary>Uncategorised attachments.</summary>
    public const string General = "general";
}
