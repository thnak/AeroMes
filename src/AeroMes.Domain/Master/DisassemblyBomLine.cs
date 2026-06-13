using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public enum DisassemblyComponentType { Main, ByProduct }

public class DisassemblyBomLine : Entity
{
    public int LineId { get; private set; }
    public int DisassemblyBomId { get; private set; }
    public int LineNo { get; private set; }
    public string ComponentCode { get; private set; } = string.Empty;
    public DisassemblyComponentType ComponentType { get; private set; }
    public decimal? RecoveryRate { get; private set; }
    public decimal? FixedQuantity { get; private set; }
    public string UoMCode { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    public Product? Component { get; private set; }

    private DisassemblyBomLine() { }

    internal static DisassemblyBomLine Create(
        int disassemblyBomId, int lineNo, string componentCode,
        DisassemblyComponentType componentType, decimal? recoveryRate,
        decimal? fixedQuantity, string uomCode, string? notes)
    {
        if (recoveryRate.HasValue && (recoveryRate < 0 || recoveryRate > 100))
            throw new DomainException($"Tỷ lệ thu hồi phải trong khoảng 0–100%. Dòng {lineNo}.");

        if (fixedQuantity.HasValue && fixedQuantity <= 0)
            throw new DomainException($"Số lượng cố định phải lớn hơn 0. Dòng {lineNo}.");

        return new DisassemblyBomLine
        {
            DisassemblyBomId = disassemblyBomId,
            LineNo = lineNo,
            ComponentCode = componentCode.Trim().ToUpperInvariant(),
            ComponentType = componentType,
            RecoveryRate = recoveryRate,
            FixedQuantity = fixedQuantity,
            UoMCode = uomCode.Trim().ToUpperInvariant(),
            Notes = notes,
        };
    }
}
