using System.Text.RegularExpressions;

namespace AeroMes.Application.Master.Products;

/// <summary>
/// Validates parameterized quantity formulas like "[Height]*[Width]*[Qty]".
/// Variables must come from the product's declared dimension fields; the
/// remainder may only contain numbers, arithmetic operators, and parentheses.
/// </summary>
public static partial class QuantityFormulaValidator
{
    public static readonly IReadOnlySet<string> AllowedVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Length", "Width", "Height", "NetWeight", "GrossWeight", "Qty",
    };

    [GeneratedRegex(@"\[([^\[\]]*)\]")]
    private static partial Regex VariableToken();

    [GeneratedRegex(@"^[0-9+\-*/().,\s]*$")]
    private static partial Regex ResidualChars();

    public static bool IsValid(string formula)
    {
        var variables = VariableToken().Matches(formula);
        if (variables.Count == 0)
            return false;
        if (variables.Any(m => !AllowedVariables.Contains(m.Groups[1].Value)))
            return false;

        var residual = VariableToken().Replace(formula, "0");
        return ResidualChars().IsMatch(residual);
    }
}
