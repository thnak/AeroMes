namespace AeroMes.Infrastructure.Iot.Modbus;

public record ModbusAddress(string RegisterType, ushort Address, string DataType, int Length = 1);

public static class ModbusAddressParser
{
    /// <summary>
    /// Parses a Modbus source address string of the form "HR:40001:FLOAT32" or "IR:30005:UINT16[:length]".
    /// Register types: CO (coils), DI (discrete inputs), IR (input registers), HR (holding registers).
    /// </summary>
    public static ModbusAddress? Parse(string sourceAddress)
    {
        if (string.IsNullOrWhiteSpace(sourceAddress)) return null;

        var parts = sourceAddress.Split(':');
        if (parts.Length < 3) return null;

        var regType = parts[0].ToUpperInvariant();
        if (regType is not ("CO" or "DI" or "IR" or "HR")) return null;

        if (!ushort.TryParse(parts[1], out var address)) return null;

        var dataType = parts[2].ToUpperInvariant();
        var length = parts.Length > 3 && int.TryParse(parts[3], out var l) && l > 0 ? l : 1;

        return new ModbusAddress(regType, address, dataType, length);
    }

    /// <summary>
    /// Returns the number of 16-bit registers required to hold a value of the given data type.
    /// For STRING, length is the number of characters.
    /// </summary>
    public static int RegisterCount(string dataType, int length) => dataType switch
    {
        "BOOL" or "INT16" or "UINT16" => 1,
        "INT32" or "UINT32" or "FLOAT32" => 2,
        "FLOAT64" or "INT64" or "UINT64" => 4,
        "STRING" => (length + 1) / 2,
        _ => 1,
    };
}
