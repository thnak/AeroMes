using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Queries.GetStandardSetDetail;

public record GetStandardSetDetailQuery(int StandardSetID) : IQuery<StandardSetDetailDto?>;
