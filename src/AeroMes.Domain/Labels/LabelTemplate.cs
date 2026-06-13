namespace AeroMes.Domain.Labels;

public sealed class LabelTemplate
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = "";
    public string PaperSize { get; private set; } = "100x50mm";
    public string Orientation { get; private set; } = "Portrait";
    public string BarcodeType { get; private set; } = "QRCode";
    public int BarcodeWidth { get; private set; } = 150;
    public int BarcodeHeight { get; private set; } = 150;
    public string[] SelectedFields { get; private set; } = [];
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private LabelTemplate() { }

    public static LabelTemplate Create(
        string name,
        string paperSize,
        string orientation,
        string barcodeType,
        int barcodeWidth,
        int barcodeHeight,
        string[] selectedFields,
        bool isDefault)
    {
        return new LabelTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            PaperSize = paperSize,
            Orientation = orientation,
            BarcodeType = barcodeType,
            BarcodeWidth = barcodeWidth,
            BarcodeHeight = barcodeHeight,
            SelectedFields = selectedFields,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        string name,
        string paperSize,
        string orientation,
        string barcodeType,
        int barcodeWidth,
        int barcodeHeight,
        string[] selectedFields,
        bool isDefault)
    {
        Name = name;
        PaperSize = paperSize;
        Orientation = orientation;
        BarcodeType = barcodeType;
        BarcodeWidth = barcodeWidth;
        BarcodeHeight = barcodeHeight;
        SelectedFields = selectedFields;
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }
}
