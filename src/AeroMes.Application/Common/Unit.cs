namespace AeroMes.Application.Common;

public sealed record Unit
{
    public static readonly Unit Value = new();
    private Unit() { }
}
