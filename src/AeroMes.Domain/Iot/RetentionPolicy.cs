namespace AeroMes.Domain.Iot;

public class RetentionPolicy
{
    public int PolicyId { get; private set; }
    public string Scope { get; private set; } = "GLOBAL";  // GLOBAL | MACHINE | TAG
    public string? ScopeValue { get; private set; }        // MachineCode or TagKey
    public int RawRetentionDays { get; private set; } = 30;
    public int Agg1minRetentionDays { get; private set; } = 90;
    public int Agg1hrRetentionDays { get; private set; } = 730;

    private RetentionPolicy() { }

    public static RetentionPolicy CreateGlobal(int raw, int agg1min, int agg1hr)
        => new() { Scope = "GLOBAL", RawRetentionDays = raw,
                   Agg1minRetentionDays = agg1min, Agg1hrRetentionDays = agg1hr };

    public void Update(int raw, int agg1min, int agg1hr)
    {
        RawRetentionDays = raw; Agg1minRetentionDays = agg1min; Agg1hrRetentionDays = agg1hr;
    }
}
