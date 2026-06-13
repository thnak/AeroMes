using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Cost;

public enum MachineCostRateType { ENERGY, DEPRECIATION, OVERHEAD, MAINTENANCE_RESERVE }

public class MachineCostRate : Entity
{
    public int RateID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public MachineCostRateType RateType { get; private set; }
    public decimal RatePerHour { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string? Notes { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private MachineCostRate() { }

    public static MachineCostRate Create(
        string machineCode, MachineCostRateType rateType, decimal ratePerHour,
        DateOnly effectiveFrom, string? notes, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(machineCode)) throw new DomainException("Mã máy không được để trống.");
        if (ratePerHour <= 0) throw new DomainException("Đơn giá giờ phải lớn hơn 0.");
        return new MachineCostRate
        {
            MachineCode = machineCode.Trim(), RateType = rateType,
            RatePerHour = ratePerHour, EffectiveFrom = effectiveFrom,
            Notes = notes, CreatedBy = createdBy
        };
    }

    public void ExpireOn(DateOnly expiryDate)
    {
        EffectiveTo = expiryDate;
    }
}
