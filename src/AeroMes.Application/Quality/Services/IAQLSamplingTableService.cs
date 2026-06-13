namespace AeroMes.Application.Quality.Services;

public record AQLSamplingPlan(int SampleSize, int AcceptanceNumber, int RejectionNumber);

public interface IAQLSamplingTableService
{
    AQLSamplingPlan GetSamplingPlan(int lotSize, string aqlLevel, string inspectionLevel);
}
