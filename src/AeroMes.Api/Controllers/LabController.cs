using AeroMes.Api.Auth;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Lab;
using AeroMes.Domain.Lab.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/lab")]
[Authorize]
public class LabController(ILabRepository labRepo, IUnitOfWork unitOfWork) : ControllerBase
{
    // ── Test Methods ──────────────────────────────────────────────────────────

    [HttpGet("methods")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<List<TestMethodDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMethods([FromQuery] string? category, [FromQuery] bool? isActive, CancellationToken ct)
    {
        var methods = await labRepo.GetMethodsAsync(category, isActive, ct);
        return Ok(methods.Select(MapMethod).ToList());
    }

    [HttpPost("methods")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<TestMethodDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateMethod([FromBody] UpsertMethodRequest req, CancellationToken ct)
    {
        var method = TestMethod.Create(req.Code, req.Name, req.Category, req.Unit,
            req.MeasurementType, req.SpecMin, req.SpecMax, req.SpecNominal,
            req.ReferenceStd, req.InstrumentType, req.EstDurationMin);
        labRepo.AddMethod(method);
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetMethods), MapMethod(method));
    }

    [HttpPut("methods/{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<TestMethodDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMethod(int id, [FromBody] UpsertMethodRequest req, CancellationToken ct)
    {
        var method = await labRepo.GetMethodByIdAsync(id, ct);
        if (method is null) return NotFound();
        method.Update(req.Name, req.Category, req.Unit, req.MeasurementType,
            req.SpecMin, req.SpecMax, req.SpecNominal, req.ReferenceStd, req.InstrumentType, req.EstDurationMin);
        await unitOfWork.SaveChangesAsync(ct);
        return Ok(MapMethod(method));
    }

    // ── Test Panels ───────────────────────────────────────────────────────────

    [HttpGet("panels")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<List<TestPanelDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPanels([FromQuery] bool? isActive, CancellationToken ct)
    {
        var panels = await labRepo.GetPanelsAsync(isActive, ct);
        return Ok(panels.Select(MapPanel).ToList());
    }

    [HttpPost("panels")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<TestPanelDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePanel([FromBody] UpsertPanelRequest req, CancellationToken ct)
    {
        var panel = TestPanel.Create(req.Code, req.Name, req.ProductCode);
        panel.SetItems(req.Items.Select((item, i) =>
            TestPanelItem.Create(0, item.TestMethodId, i + 1, item.IsRequired, item.SpecOverrideMin, item.SpecOverrideMax)));
        labRepo.AddPanel(panel);
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetPanels), MapPanel(panel));
    }

    [HttpPut("panels/{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<TestPanelDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePanel(int id, [FromBody] UpsertPanelRequest req, CancellationToken ct)
    {
        var panel = await labRepo.GetPanelByIdAsync(id, ct);
        if (panel is null) return NotFound();
        panel.Update(req.Name, req.ProductCode);
        panel.SetItems(req.Items.Select((item, i) =>
            TestPanelItem.Create(id, item.TestMethodId, i + 1, item.IsRequired, item.SpecOverrideMin, item.SpecOverrideMax)));
        await unitOfWork.SaveChangesAsync(ct);
        return Ok(MapPanel(panel));
    }

    // ── Lab Requests ──────────────────────────────────────────────────────────

    [HttpGet("requests")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<List<LabRequestListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequests(
        [FromQuery] string? status, [FromQuery] string? priority, [FromQuery] string? requestType,
        [FromQuery] string? productCode, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to,
        CancellationToken ct)
    {
        var requests = await labRepo.GetRequestsAsync(status, priority, requestType, productCode, from, to, ct);
        return Ok(requests.Select(MapRequestList).ToList());
    }

    [HttpGet("requests/{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<LabRequestDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequest(int id, CancellationToken ct)
    {
        var request = await labRepo.GetRequestByIdAsync(id, ct);
        if (request is null) return NotFound();
        var samples = await labRepo.GetSamplesForRequestAsync(id, ct);
        var results = await labRepo.GetResultsForRequestAsync(id, ct);
        var report = await labRepo.GetReportForRequestAsync(id, ct);
        return Ok(new LabRequestDetailDto(
            request.RequestId, request.RequestNo, request.RequestType, request.Status,
            request.Priority, request.ProductCode, request.LotNumber, request.WorkOrderId,
            request.CustomerCode, request.PanelId, request.SampleQty, request.SampleUnit,
            request.SampleLocation, request.RequiredBy, request.RequestedBy, request.RequestedAt,
            request.AssignedTo, request.Notes,
            samples.Select(MapSample).ToList(),
            results.Select(MapResult).ToList(),
            report is not null ? MapReport(report) : null));
    }

    [HttpPost("requests")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<LabRequestListDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestDto req, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var seq = await labRepo.GetNextRequestSeqAsync(year, ct);
        var requestNo = $"LAB-{year}-{seq:D5}";
        var request = LabRequest.Create(requestNo, req.RequestType, req.Priority,
            req.ProductCode, req.LotNumber, req.WorkOrderId, req.InspectionOrderId,
            req.CustomerCode, req.PanelId, req.SampleQty, req.SampleUnit,
            req.SampleLocation, req.RequiredBy, User.Identity?.Name ?? "system", req.Notes);
        labRepo.AddRequest(request);
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetRequest), new { id = request.RequestId }, MapRequestList(request));
    }

    [HttpPatch("requests/{id:int}/assign")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRequest(int id, [FromBody] AssignRequestDto req, CancellationToken ct)
    {
        var request = await labRepo.GetRequestByIdAsync(id, ct);
        if (request is null) return NotFound();
        request.Assign(req.TechnicianName);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("requests/{id:int}/cancel")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelRequest(int id, CancellationToken ct)
    {
        var request = await labRepo.GetRequestByIdAsync(id, ct);
        if (request is null) return NotFound();
        request.Cancel();
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Samples ───────────────────────────────────────────────────────────────

    [HttpPost("requests/{id:int}/samples")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<LabSampleDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSample(int id, [FromBody] AddSampleDto req, CancellationToken ct)
    {
        var request = await labRepo.GetRequestByIdAsync(id, ct);
        if (request is null) return NotFound();
        var sample = LabSample.Create(id, req.SampleCode, req.ConditionOnReceipt,
            User.Identity?.Name ?? "system", req.StorageLocation);
        labRepo.AddSample(sample);
        if (request.Status == "PENDING") request.BeginSampling();
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetRequest), new { id }, MapSample(sample));
    }

    [HttpPatch("samples/{sampleId:long}/dispose")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisposeSample(long sampleId, [FromBody] DisposeSampleDto req, CancellationToken ct)
    {
        var sample = await labRepo.GetSampleByIdAsync(sampleId, ct);
        if (sample is null) return NotFound();
        sample.Dispose(req.DisposalMethod);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Results ───────────────────────────────────────────────────────────────

    [HttpPut("requests/{id:int}/results")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<List<LabResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertResults(int id, [FromBody] List<UpsertResultDto> req, CancellationToken ct)
    {
        var request = await labRepo.GetRequestByIdAsync(id, ct);
        if (request is null) return NotFound();

        var created = new List<TestResult>();
        foreach (var r in req)
        {
            bool? withinSpec = null;
            if (r.MeasuredValue.HasValue)
            {
                var method = await labRepo.GetMethodByIdAsync(r.TestMethodId, ct);
                if (method is not null && method.SpecMin.HasValue && method.SpecMax.HasValue)
                    withinSpec = r.MeasuredValue >= method.SpecMin && r.MeasuredValue <= method.SpecMax;
            }
            else if (!string.IsNullOrEmpty(r.AttributeResult))
                withinSpec = r.AttributeResult == "PASS";

            var result = TestResult.Record(id, r.SampleId, r.TestMethodId,
                r.MeasuredValue, r.AttributeResult, withinSpec,
                r.InstrumentCode, User.Identity?.Name ?? "system", r.Notes);
            labRepo.AddResult(result);
            created.Add(result);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Ok(created.Select(MapResult).ToList());
    }

    [HttpPost("requests/{id:int}/results/{resultId:long}/review")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewResult(int id, long resultId, CancellationToken ct)
    {
        var result = await labRepo.GetResultByIdAsync(resultId, ct);
        if (result is null || result.RequestId != id) return NotFound();
        result.Review(User.Identity?.Name ?? "system");
        labRepo.UpdateResult(result);
        await unitOfWork.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Reports / CoA ─────────────────────────────────────────────────────────

    [HttpPost("requests/{id:int}/report")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<LabReportDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> IssueReport(int id, [FromBody] IssueReportDto req, CancellationToken ct)
    {
        var request = await labRepo.GetRequestByIdAsync(id, ct);
        if (request is null) return NotFound();
        var results = await labRepo.GetResultsForRequestAsync(id, ct);
        if (results.Count == 0)
            return UnprocessableEntity(new ProblemDetails { Title = "No test results recorded yet." });

        var overallResult = results.All(r => r.IsWithinSpec == true) ? "PASS" : "FAIL";
        var year = DateTime.UtcNow.Year;
        var seq = await labRepo.GetNextReportSeqAsync(year, ct);
        var reportNo = $"COA-{year}-{seq:D5}";

        var report = LabReport.Issue(reportNo, id, overallResult, req.Conclusion,
            User.Identity?.Name ?? "system", request.CustomerCode);
        labRepo.AddReport(report);
        request.Complete();
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetReport), new { reportId = report.ReportId }, MapReport(report));
    }

    [HttpGet("reports/{reportId:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<LabReportDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport(int reportId, CancellationToken ct)
    {
        var report = await labRepo.GetReportByIdAsync(reportId, ct);
        if (report is null) return NotFound();
        return Ok(MapReport(report));
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static TestMethodDto MapMethod(TestMethod m) => new(
        m.TestMethodId, m.Code, m.Name, m.Category, m.Unit, m.MeasurementType,
        m.SpecMin, m.SpecMax, m.SpecNominal, m.ReferenceStd, m.InstrumentType,
        m.EstDurationMin, m.IsActive, m.CreatedAt, m.UpdatedAt);

    private static TestPanelDto MapPanel(TestPanel p) => new(
        p.PanelId, p.Code, p.Name, p.ProductCode, p.IsActive, p.CreatedAt, p.UpdatedAt,
        p.Items.Select(i => new TestPanelItemDto(i.PanelItemId, i.TestMethodId, i.Sequence,
            i.IsRequired, i.SpecOverrideMin, i.SpecOverrideMax)).ToList());

    private static LabRequestListDto MapRequestList(LabRequest r) => new(
        r.RequestId, r.RequestNo, r.RequestType, r.Status, r.Priority,
        r.ProductCode, r.LotNumber, r.AssignedTo, r.RequiredBy, r.RequestedAt, r.RequestedBy);

    private static LabSampleDto MapSample(LabSample s) => new(
        s.SampleId, s.RequestId, s.SampleCode, s.ConditionOnReceipt,
        s.ReceivedBy, s.ReceivedAt, s.StorageLocation, s.DisposedAt, s.DisposalMethod);

    private static LabResultDto MapResult(TestResult r) => new(
        r.ResultId, r.RequestId, r.SampleId, r.TestMethodId, r.MeasuredValue,
        r.AttributeResult, r.IsWithinSpec, r.InstrumentCode, r.TestedBy, r.TestedAt,
        r.ReviewedBy, r.ReviewedAt, r.Notes);

    private static LabReportDto MapReport(LabReport r) => new(
        r.ReportId, r.ReportNo, r.RequestId, r.OverallResult, r.Conclusion,
        r.IssuedBy, r.IssuedAt, r.CustomerCode, r.DocumentUrl);
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record TestMethodDto(int TestMethodId, string Code, string Name, string Category,
    string Unit, string MeasurementType, decimal? SpecMin, decimal? SpecMax, decimal? SpecNominal,
    string? ReferenceStd, string? InstrumentType, int? EstDurationMin, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);

public record TestPanelDto(int PanelId, string Code, string Name, string? ProductCode,
    bool IsActive, DateTime CreatedAt, DateTime UpdatedAt, List<TestPanelItemDto> Items);

public record TestPanelItemDto(int PanelItemId, int TestMethodId, int Sequence,
    bool IsRequired, decimal? SpecOverrideMin, decimal? SpecOverrideMax);

public record LabRequestListDto(int RequestId, string RequestNo, string RequestType,
    string Status, string Priority, string ProductCode, string? LotNumber,
    string? AssignedTo, DateTimeOffset? RequiredBy, DateTimeOffset RequestedAt, string RequestedBy);

public record LabRequestDetailDto(int RequestId, string RequestNo, string RequestType,
    string Status, string Priority, string ProductCode, string? LotNumber,
    long? WorkOrderId, string? CustomerCode, int? PanelId, decimal SampleQty, string SampleUnit,
    string? SampleLocation, DateTimeOffset? RequiredBy, string RequestedBy, DateTimeOffset RequestedAt,
    string? AssignedTo, string? Notes,
    List<LabSampleDto> Samples, List<LabResultDto> Results, LabReportDto? Report);

public record LabSampleDto(long SampleId, int RequestId, string SampleCode,
    string ConditionOnReceipt, string ReceivedBy, DateTimeOffset ReceivedAt,
    string? StorageLocation, DateTimeOffset? DisposedAt, string? DisposalMethod);

public record LabResultDto(long ResultId, int RequestId, long SampleId, int TestMethodId,
    decimal? MeasuredValue, string? AttributeResult, bool? IsWithinSpec,
    string? InstrumentCode, string TestedBy, DateTimeOffset TestedAt,
    string? ReviewedBy, DateTimeOffset? ReviewedAt, string? Notes);

public record LabReportDto(int ReportId, string ReportNo, int RequestId, string OverallResult,
    string Conclusion, string IssuedBy, DateTimeOffset IssuedAt, string? CustomerCode, string? DocumentUrl);

public record UpsertMethodRequest(string Code, string Name, string Category, string Unit,
    string MeasurementType, decimal? SpecMin, decimal? SpecMax, decimal? SpecNominal,
    string? ReferenceStd, string? InstrumentType, int? EstDurationMin);

public record PanelItemSpec(int TestMethodId, bool IsRequired = true,
    decimal? SpecOverrideMin = null, decimal? SpecOverrideMax = null);

public record UpsertPanelRequest(string Code, string Name, string? ProductCode, List<PanelItemSpec> Items);

public record CreateRequestDto(string RequestType, string Priority, string ProductCode,
    string? LotNumber, long? WorkOrderId, int? InspectionOrderId, string? CustomerCode,
    int? PanelId, decimal SampleQty, string SampleUnit, string? SampleLocation,
    DateTimeOffset? RequiredBy, string? Notes);

public record AssignRequestDto(string TechnicianName);

public record AddSampleDto(string SampleCode, string ConditionOnReceipt, string? StorageLocation);

public record DisposeSampleDto(string DisposalMethod);

public record UpsertResultDto(long SampleId, int TestMethodId, decimal? MeasuredValue,
    string? AttributeResult, string? InstrumentCode, string? Notes);

public record IssueReportDto(string Conclusion);
