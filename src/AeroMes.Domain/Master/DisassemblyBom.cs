using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public enum DisassemblyBomType { FixedRatio, FixedQuantity }
public enum DisassemblyBomStatus { Active, Inactive }

public class DisassemblyBom : AuditableEntity
{
    public int DisassemblyBomId { get; private set; }
    public string BomCode { get; private set; } = string.Empty;
    public string BomName { get; private set; } = string.Empty;
    public string SourceProductCode { get; private set; } = string.Empty;
    public DisassemblyBomType BomType { get; private set; }
    public decimal LossRatio { get; private set; }
    public bool IsDefault { get; private set; }
    public DisassemblyBomStatus Status { get; private set; } = DisassemblyBomStatus.Active;
    public DateOnly? EffectiveDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }

    public Product? SourceProduct { get; private set; }

    private readonly List<DisassemblyBomLine> _lines = [];
    public IReadOnlyList<DisassemblyBomLine> Lines => _lines.AsReadOnly();

    private DisassemblyBom() { }

    public static DisassemblyBom Create(
        string bomCode, string bomName, string sourceProductCode,
        DisassemblyBomType bomType, decimal lossRatio,
        DateOnly? effectiveDate, DateOnly? expiryDate, string? createdBy)
    {
        if (lossRatio < 0 || lossRatio > 100)
            throw new DomainException("Tỷ lệ hao hụt phải trong khoảng 0–100%.");
        if (effectiveDate.HasValue && expiryDate.HasValue && effectiveDate > expiryDate)
            throw new DomainException("Ngày hiệu lực không được sau ngày hết hạn.");

        return new DisassemblyBom
        {
            BomCode = bomCode.Trim().ToUpperInvariant(),
            BomName = bomName.Trim(),
            SourceProductCode = sourceProductCode.Trim().ToUpperInvariant(),
            BomType = bomType,
            LossRatio = lossRatio,
            Status = DisassemblyBomStatus.Active,
            EffectiveDate = effectiveDate,
            ExpiryDate = expiryDate,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        string bomName, decimal lossRatio,
        DateOnly? effectiveDate, DateOnly? expiryDate, string? updatedBy)
    {
        if (lossRatio < 0 || lossRatio > 100)
            throw new DomainException("Tỷ lệ hao hụt phải trong khoảng 0–100%.");
        if (effectiveDate.HasValue && expiryDate.HasValue && effectiveDate > expiryDate)
            throw new DomainException("Ngày hiệu lực không được sau ngày hết hạn.");

        BomName = bomName.Trim();
        LossRatio = lossRatio;
        EffectiveDate = effectiveDate;
        ExpiryDate = expiryDate;
        Touch(updatedBy);
    }

    public void ReplaceLines(
        IReadOnlyList<(int LineNo, string ComponentCode, DisassemblyComponentType ComponentType,
            decimal? RecoveryRate, decimal? FixedQuantity, string UoMCode, string? Notes)> lines,
        string? updatedBy)
    {
        ValidateLines(lines);
        _lines.Clear();
        foreach (var line in lines)
            _lines.Add(DisassemblyBomLine.Create(
                DisassemblyBomId, line.LineNo, line.ComponentCode, line.ComponentType,
                line.RecoveryRate, line.FixedQuantity, line.UoMCode, line.Notes));
        Touch(updatedBy);
    }

    private void ValidateLines(
        IReadOnlyList<(int LineNo, string ComponentCode, DisassemblyComponentType ComponentType,
            decimal? RecoveryRate, decimal? FixedQuantity, string UoMCode, string? Notes)> lines)
    {
        if (lines.Count == 0 || !lines.Any(l => l.ComponentType == DisassemblyComponentType.Main))
            throw new DomainException("Phải có ít nhất một dòng thành phần chính (Main).");

        foreach (var line in lines)
        {
            if (BomType == DisassemblyBomType.FixedRatio && !line.RecoveryRate.HasValue)
                throw new DomainException($"Dòng {line.LineNo}: BOM loại FixedRatio yêu cầu tỷ lệ thu hồi.");
            if (BomType == DisassemblyBomType.FixedQuantity && !line.FixedQuantity.HasValue)
                throw new DomainException($"Dòng {line.LineNo}: BOM loại FixedQuantity yêu cầu số lượng cố định.");
        }
    }

    public void SetAsDefault(string? updatedBy) { IsDefault = true; Touch(updatedBy); }
    public void ClearDefault(string? updatedBy) { IsDefault = false; Touch(updatedBy); }
    public void Activate(string? updatedBy) { Status = DisassemblyBomStatus.Active; Touch(updatedBy); }
    public void Deactivate(string? updatedBy) { Status = DisassemblyBomStatus.Inactive; Touch(updatedBy); }
}
