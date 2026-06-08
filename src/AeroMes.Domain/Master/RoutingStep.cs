using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Master;

public class RoutingStep : Entity
{
    public int RoutingStepID { get; private set; }
    public int RoutingID { get; private set; }
    public int StepNumber { get; private set; }           // sequential order (1, 2, 3…)
    public string OperationCode { get; private set; } = string.Empty;
    public int DefaultWorkCenterID { get; private set; }
    public double StandardCycleTime { get; private set; } // seconds per unit
    public bool IsQcRequired { get; private set; }

    // EF navigations
    public Routing? Routing { get; private set; }
    public Operation? Operation { get; private set; }
    public WorkCenter? DefaultWorkCenter { get; private set; }

    private RoutingStep() { }

    public static RoutingStep Create(
        int routingId,
        int stepNumber,
        string operationCode,
        int defaultWorkCenterId,
        double standardCycleTime = 0,
        bool isQcRequired = false)
    {
        if (stepNumber <= 0)
            throw new DomainException($"Step number must be positive. Got: {stepNumber}.");
        if (string.IsNullOrWhiteSpace(operationCode))
            throw new DomainException("Operation code is required.");
        if (standardCycleTime < 0)
            throw new DomainException("Standard cycle time cannot be negative.");

        return new RoutingStep
        {
            RoutingID = routingId,
            StepNumber = stepNumber,
            OperationCode = operationCode.Trim().ToUpperInvariant(),
            DefaultWorkCenterID = defaultWorkCenterId,
            StandardCycleTime = standardCycleTime,
            IsQcRequired = isQcRequired,
        };
    }
}
