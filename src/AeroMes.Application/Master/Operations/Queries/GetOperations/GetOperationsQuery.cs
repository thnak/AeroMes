using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Operations.Queries.GetOperations;

public record GetOperationsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<OperationDto>>;

public record OperationDto(
    string OperationCode,
    string OperationName,
    string? Description,
    bool IsActive);
