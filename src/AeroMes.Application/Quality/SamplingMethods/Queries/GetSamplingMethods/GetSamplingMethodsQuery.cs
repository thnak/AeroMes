using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.SamplingMethods.Queries.GetSamplingMethods;

public record GetSamplingMethodsQuery(bool? ActiveOnly) : IQuery<IReadOnlyList<SamplingMethodDto>>;
