using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Queries.GetEffectiveStandardSet;

public record GetEffectiveStandardSetQuery(string ProductCode, DateOnly Date) : IQuery<StandardSetDetailDto?>;
