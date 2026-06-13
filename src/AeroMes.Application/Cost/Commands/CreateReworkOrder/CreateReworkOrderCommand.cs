using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.Commands.CreateReworkOrder;

public record CreateReworkOrderCommand(
    string ReworkCode,
    int SourceWOID,
    long? ScrapTxID,
    string ProductCode,
    int ReworkQty,
    int? ReworkStepFromId,
    string CreatedBy) : ICommand<ValidationResult<int>>;
