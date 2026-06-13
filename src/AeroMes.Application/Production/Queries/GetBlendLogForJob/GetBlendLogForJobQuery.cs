using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetBlendLogForJob;

public record GetBlendLogForJobQuery(long JobID) : IQuery<IReadOnlyList<MaterialBlendLogDto>>;
