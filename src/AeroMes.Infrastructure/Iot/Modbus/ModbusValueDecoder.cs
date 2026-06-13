using System.Buffers.Binary;
using System.Text;

namespace AeroMes.Infrastructure.Iot.Modbus;

/// <summary>
/// Decodes raw Modbus register bytes into decimal values, respecting byte order and word order.
/// </summary>
public static class ModbusValueDecoder
{
    /// <summary>
    /// Decodes a Modbus register byte sequence into a decimal value.
    /// </summary>
    /// <param name="bytes">Raw bytes from the Modbus device (register data in wire order).</param>
    /// <param name="dataType">Target data type: BOOL, INT16, UINT16, INT32, UINT32, FLOAT32, FLOAT64, STRING.</param>
    /// <param name="byteOrder">
    ///   "BigEndian" (default) — MSB first within each 16-bit register (standard Modbus).
    ///   "LittleEndian" — LSB first within each 16-bit register.
    /// </param>
    /// <param name="wordOrder">
    ///   "HighWordFirst" (default) — most-significant 16-bit word comes first in memory.
    ///   "LowWordFirst" — least-significant 16-bit word comes first.
    /// </param>
    public static decimal Decode(
        ReadOnlySpan<byte> bytes,
        string dataType,
        string byteOrder,
        string wordOrder)
    {
        if (bytes.IsEmpty) return 0m;

        try
        {
            // Normalise into a local mutable buffer with the desired byte arrangement.
            Span<byte> buf = stackalloc byte[bytes.Length];
            bytes.CopyTo(buf);

            ApplyByteOrder(buf, byteOrder);
            ApplyWordOrder(buf, wordOrder);

            return dataType switch
            {
                "BOOL" => (buf[0] != 0 || (buf.Length > 1 && buf[1] != 0)) ? 1m : 0m,
                "INT16" => buf.Length >= 2 ? (decimal)BinaryPrimitives.ReadInt16BigEndian(buf) : 0m,
                "UINT16" => buf.Length >= 2 ? (decimal)BinaryPrimitives.ReadUInt16BigEndian(buf) : 0m,
                "INT32" => buf.Length >= 4 ? (decimal)BinaryPrimitives.ReadInt32BigEndian(buf) : 0m,
                "UINT32" => buf.Length >= 4 ? (decimal)BinaryPrimitives.ReadUInt32BigEndian(buf) : 0m,
                "FLOAT32" => buf.Length >= 4
                    ? (decimal)BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(buf))
                    : 0m,
                "FLOAT64" => buf.Length >= 8
                    ? (decimal)BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(buf))
                    : 0m,
                "INT64" => buf.Length >= 8 ? (decimal)BinaryPrimitives.ReadInt64BigEndian(buf) : 0m,
                "UINT64" => buf.Length >= 8 ? (decimal)BinaryPrimitives.ReadUInt64BigEndian(buf) : 0m,
                "STRING" => 0m, // strings should be read separately; return 0 as a sentinel
                _ => 0m,
            };
        }
        catch
        {
            return 0m;
        }
    }

    /// <summary>
    /// Decodes a Modbus register byte sequence into a string value (ASCII).
    /// </summary>
    public static string DecodeString(ReadOnlySpan<byte> bytes, string byteOrder, string wordOrder)
    {
        if (bytes.IsEmpty) return string.Empty;

        try
        {
            Span<byte> buf = stackalloc byte[bytes.Length];
            bytes.CopyTo(buf);
            ApplyByteOrder(buf, byteOrder);
            ApplyWordOrder(buf, wordOrder);

            return Encoding.ASCII.GetString(buf).TrimEnd('\0');
        }
        catch
        {
            return string.Empty;
        }
    }

    // ── Byte order ────────────────────────────────────────────────────────────

    /// <summary>
    /// Applies byte order normalisation. After this call, bytes within each 16-bit word are
    /// in big-endian order (MSB first), which is what BinaryPrimitives expects.
    /// BigEndian is the standard Modbus wire format — no swap needed.
    /// LittleEndian swaps bytes within every 2-byte pair.
    /// </summary>
    private static void ApplyByteOrder(Span<byte> buf, string byteOrder)
    {
        if (!string.Equals(byteOrder, "LittleEndian", StringComparison.OrdinalIgnoreCase))
            return; // BigEndian is the default — no change

        for (var i = 0; i + 1 < buf.Length; i += 2)
            (buf[i], buf[i + 1]) = (buf[i + 1], buf[i]);
    }

    /// <summary>
    /// Applies word order normalisation for multi-register values (4+ bytes).
    /// HighWordFirst is standard — no change. LowWordFirst swaps adjacent 16-bit words.
    /// </summary>
    private static void ApplyWordOrder(Span<byte> buf, string wordOrder)
    {
        if (buf.Length < 4) return; // single-register values are unaffected
        if (!string.Equals(wordOrder, "LowWordFirst", StringComparison.OrdinalIgnoreCase))
            return; // HighWordFirst is the default — no change

        // Swap each pair of 16-bit words: [W0 W1 W2 W3] becomes [W2 W3 W0 W1], etc.
        for (var i = 0; i + 3 < buf.Length; i += 4)
        {
            (buf[i], buf[i + 2]) = (buf[i + 2], buf[i]);
            (buf[i + 1], buf[i + 3]) = (buf[i + 3], buf[i + 1]);
        }
    }
}
