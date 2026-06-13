using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.Commands.PostScrap;

public record PostScrapCommand(
    int WOID,
    long? LogID,
    int? DefectCodeId,
    string ProductCode,
    string? LotNumber,
    int ScrapQty,
    decimal MaterialCostPerUnit,
    decimal LaborCostSunk,
    decimal MachineCostSunk,
    DisposalMethod DisposalMethod,
    int? ScrapLocationId,
    string? ApprovedBy,
    string? Notes,
    string? CreatedBy) : ICommand<ValidationResult<long>>;
