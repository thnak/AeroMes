using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetRecall;

public sealed record GetRecallQuery(Guid RecallID) : IQuery<RecallDetailDto?>;
