using MediatR;

namespace AeroMes.Application.Master.Operations.Queries.GetOperations;

public record GetOperationsQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<OperationDto>>;

public record OperationDto(
    string OperationCode,
    string OperationName,
    string? Description,
    bool IsActive);
