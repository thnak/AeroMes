using AeroMes.Domain.Common;

namespace AeroMes.Domain.Master;

public class MachineProductParam : Entity
{
    public int ParamId { get; private set; }
    public string MachineCode { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public string ParamName { get; private set; } = string.Empty;
    public string? Unit { get; private set; }
    public decimal? NominalValue { get; private set; }
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }
    public bool IsControlParam { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    private MachineProductParam() { }

    public static MachineProductParam Create(
        string machineCode,
        string productCode,
        string paramName,
        string? unit,
        decimal? nominalValue,
        decimal? minValue,
        decimal? maxValue,
        bool isControlParam,
        int displayOrder)
    {
        return new MachineProductParam
        {
            MachineCode = machineCode.Trim().ToUpperInvariant(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            ParamName = paramName.Trim(),
            Unit = unit,
            NominalValue = nominalValue,
            MinValue = minValue,
            MaxValue = maxValue,
            IsControlParam = isControlParam,
            DisplayOrder = displayOrder,
        };
    }

    public void Update(
        string? unit,
        decimal? nominalValue,
        decimal? minValue,
        decimal? maxValue,
        bool isControlParam,
        int displayOrder)
    {
        Unit = unit;
        NominalValue = nominalValue;
        MinValue = minValue;
        MaxValue = maxValue;
        IsControlParam = isControlParam;
        DisplayOrder = displayOrder;
    }
}
