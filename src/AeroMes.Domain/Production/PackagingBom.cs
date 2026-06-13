using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production;

public class PackagingBom : Entity
{
    private readonly List<PackagingBomLine> _lines = [];

    public int PackagingBomID { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int Version { get; private set; } = 1;
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<PackagingBomLine> Lines => _lines.AsReadOnly();

    private PackagingBom() { }

    public static PackagingBom Create(string productCode, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            throw new DomainException("ProductCode is required.");
        return new PackagingBom
        {
            ProductCode = productCode.Trim().ToUpperInvariant(),
            Notes = notes,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void AddLine(string materialCode, decimal quantity, string unitCode, string? lineNotes = null)
    {
        if (quantity <= 0) throw new DomainException("Line quantity must be > 0.");
        _lines.Add(new PackagingBomLine
        {
            MaterialCode = materialCode.Trim().ToUpperInvariant(),
            Quantity = quantity,
            UnitCode = unitCode.Trim().ToUpperInvariant(),
            Notes = lineNotes,
        });
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}

public class PackagingBomLine
{
    public int LineID { get; set; }
    public int PackagingBomID { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
