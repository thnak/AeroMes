namespace AeroMes.Domain.Exceptions;

public class DifferentShadeConflictException(string fabricProductCode, IEnumerable<string> shadeCodes)
    : Exception(
        $"All fabric rolls for '{fabricProductCode}' must share the same shade code, but found: {string.Join(", ", shadeCodes)}.");
