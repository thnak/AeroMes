using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability.Events;

namespace AeroMes.Domain.Traceability;

public enum StepOutcome { InProgress, Pass, Fail, Deviation, Skipped }

public class ProcessRecord : Entity
{
    public Guid ProcessRecordID { get; private set; }
    public string LotNumber { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public int WorkOrderID { get; private set; }
    public long? JobID { get; private set; }
    public int RoutingStepID { get; private set; }
    public int StepSequence { get; private set; }
    public string StepName { get; private set; } = string.Empty;

    public string OperatorCode { get; private set; } = string.Empty;
    public string? CertificationRef { get; private set; }

    public string? MachineCode { get; private set; }
    public string? CalibrationRef { get; private set; }

    public string? BOMRevision { get; private set; }
    public string? RoutingRevision { get; private set; }
    public string? ControlPlanRev { get; private set; }

    public DateTime StepStartedAt { get; private set; }
    public DateTime? StepCompletedAt { get; private set; }
    public int? DurationSeconds { get; private set; }

    public StepOutcome StepOutcome { get; private set; } = StepOutcome.InProgress;
    public string? DeviationRef { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<ProcessParameter> _parameters = [];
    public IReadOnlyList<ProcessParameter> Parameters => _parameters.AsReadOnly();

    private ProcessRecord() { }

    public static ProcessRecord Open(
        string lotNumber,
        string productCode,
        int workOrderId,
        long? jobId,
        int routingStepId,
        int stepSequence,
        string stepName,
        string operatorCode,
        string? machineCode,
        string? bomRevision,
        string? routingRevision,
        string? controlPlanRev = null,
        string? certificationRef = null,
        string? calibrationRef = null)
    {
        if (string.IsNullOrWhiteSpace(lotNumber)) throw new DomainException("Lot number is required.");
        if (string.IsNullOrWhiteSpace(productCode)) throw new DomainException("Product code is required.");
        if (string.IsNullOrWhiteSpace(stepName)) throw new DomainException("Step name is required.");
        if (string.IsNullOrWhiteSpace(operatorCode)) throw new DomainException("Operator code is required.");

        var record = new ProcessRecord
        {
            ProcessRecordID = Guid.NewGuid(),
            LotNumber = lotNumber.Trim().ToUpperInvariant(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            WorkOrderID = workOrderId,
            JobID = jobId,
            RoutingStepID = routingStepId,
            StepSequence = stepSequence,
            StepName = stepName.Trim(),
            OperatorCode = operatorCode.Trim(),
            CertificationRef = certificationRef?.Trim(),
            MachineCode = machineCode?.Trim().ToUpperInvariant(),
            CalibrationRef = calibrationRef?.Trim(),
            BOMRevision = bomRevision?.Trim(),
            RoutingRevision = routingRevision?.Trim(),
            ControlPlanRev = controlPlanRev?.Trim(),
            StepStartedAt = DateTime.UtcNow,
            StepOutcome = StepOutcome.InProgress,
            CreatedAt = DateTime.UtcNow,
        };

        record.RaiseDomainEvent(new ProcessRecordOpenedEvent(
            record.ProcessRecordID, record.LotNumber, workOrderId, routingStepId));

        return record;
    }

    public ProcessParameter AddParameter(
        string parameterName,
        string actualValue,
        string? nominalValue,
        string? uom,
        string? lsl,
        string? usl,
        ParameterDataSource dataSource)
    {
        if (StepOutcome != StepOutcome.InProgress)
            throw new DomainException("Cannot add parameters to a closed process record.");
        if (string.IsNullOrWhiteSpace(parameterName)) throw new DomainException("Parameter name is required.");
        if (string.IsNullOrWhiteSpace(actualValue)) throw new DomainException("Actual value is required.");

        bool? inSpec = null;
        if (double.TryParse(actualValue, out var actual))
        {
            bool hasLsl = double.TryParse(lsl, out var lslVal);
            bool hasUsl = double.TryParse(usl, out var uslVal);
            if (hasLsl || hasUsl)
                inSpec = (!hasLsl || actual >= lslVal) && (!hasUsl || actual <= uslVal);
        }

        var param = ProcessParameter.Capture(
            ProcessRecordID, parameterName, nominalValue, actualValue, uom, lsl, usl, inSpec, dataSource);

        _parameters.Add(param);

        RaiseDomainEvent(new ProcessParameterCapturedEvent(ProcessRecordID, parameterName, inSpec));

        if (inSpec == false)
            RaiseDomainEvent(new ParameterOutOfSpecEvent(ProcessRecordID, parameterName, actualValue, lsl, usl));

        return param;
    }

    public void Close(StepOutcome outcome, string? deviationRef = null)
    {
        if (StepOutcome != StepOutcome.InProgress)
            throw new DomainException("Process record is already closed.");
        if (outcome == StepOutcome.InProgress)
            throw new DomainException("Cannot close a process record with outcome 'InProgress'.");

        StepOutcome = outcome;
        StepCompletedAt = DateTime.UtcNow;
        DurationSeconds = (int)(StepCompletedAt.Value - StepStartedAt).TotalSeconds;
        DeviationRef = deviationRef?.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProcessRecordClosedEvent(ProcessRecordID, LotNumber, outcome.ToString()));
    }
}
