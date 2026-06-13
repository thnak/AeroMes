using AeroMes.Domain.Quality;

namespace AeroMes.Application.Quality.InspectionResults;

public static class InspectionEvaluator
{
    public static bool EvaluateWithinSpec(
        InspectionCharacteristic characteristic,
        decimal? measuredValue,
        string? attributeResult)
    {
        if (characteristic.MeasurementType == "VARIABLE")
        {
            if (measuredValue is null) return false;
            var inMin = characteristic.SpecMin is null || measuredValue >= characteristic.SpecMin;
            var inMax = characteristic.SpecMax is null || measuredValue <= characteristic.SpecMax;
            return inMin && inMax;
        }
        // ATTRIBUTE
        return attributeResult == "PASS";
    }

    /// <summary>
    /// Returns "PASSED", "FAILED", or null (not yet determinable — still waiting for required characteristics).
    /// </summary>
    public static string? EvaluateOrderOutcome(
        InspectionPlan plan,
        IReadOnlyList<InspectionCharacteristic> requiredChars,
        IReadOnlyList<InspectionResult> allResults,
        InspectionCharacteristic? criticalFailedChar)
    {
        // Critical fail → immediate FAILED
        if (criticalFailedChar is not null) return "FAILED";

        // Count failures
        var failCount = allResults.Count(r => r.IsWithinSpec == false);
        if (failCount > plan.AcceptNumber) return "FAILED";

        // Check if all required chars have results
        var recordedCharIds = new HashSet<int>(allResults.Select(r => r.CharId));
        var allRecorded = requiredChars.All(c => recordedCharIds.Contains(c.CharId));
        if (!allRecorded) return null; // still open

        return "PASSED";
    }
}
