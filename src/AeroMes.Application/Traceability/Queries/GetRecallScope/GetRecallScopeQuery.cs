using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetRecallScope;

public sealed record GetRecallScopeQuery(Guid RecallID) : IQuery<RecallScopeDto>;
