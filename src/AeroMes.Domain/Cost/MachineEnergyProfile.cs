using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Cost;

public class MachineEnergyProfile : Entity
{
    public int ProfileID { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public decimal NominalKW { get; private set; }
    public decimal LoadFactor { get; private set; } = 0.75m;
    public int TariffID { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    private MachineEnergyProfile() { }

    public static MachineEnergyProfile Create(
        string machineCode, decimal nominalKW, decimal loadFactor,
        int tariffId, DateOnly effectiveFrom)
    {
        if (string.IsNullOrWhiteSpace(machineCode)) throw new DomainException("Mã máy không được để trống.");
        if (nominalKW <= 0) throw new DomainException("Công suất định mức phải lớn hơn 0.");
        if (loadFactor is <= 0 or > 1) throw new DomainException("Hệ số tải phải trong khoảng 0.01 đến 1.0.");
        return new MachineEnergyProfile
        {
            MachineCode = machineCode.Trim(), NominalKW = nominalKW,
            LoadFactor = loadFactor, TariffID = tariffId, EffectiveFrom = effectiveFrom
        };
    }

    public void ExpireOn(DateOnly expiryDate) => EffectiveTo = expiryDate;
}
