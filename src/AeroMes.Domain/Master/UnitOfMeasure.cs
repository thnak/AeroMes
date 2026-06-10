namespace AeroMes.Domain.Master;

public class UnitOfMeasure
{
    public string UoMCode { get; private set; } = string.Empty;
    public string UoMName { get; private set; } = string.Empty;
    public string UoMGroup { get; private set; } = string.Empty; // QUANTITY, WEIGHT, LENGTH, VOLUME

    private UnitOfMeasure() { }

    public static UnitOfMeasure Create(string code, string name, string group)
    {
        return new UnitOfMeasure
        {
            UoMCode = code.Trim().ToUpperInvariant(),
            UoMName = name.Trim(),
            UoMGroup = group.Trim().ToUpperInvariant(),
        };
    }

    public void Update(string name, string group)
    {
        UoMName = name.Trim();
        UoMGroup = group.Trim().ToUpperInvariant();
    }
}
