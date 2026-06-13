using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Queries.GetNcrDetail;

public record GetNcrDetailQuery(int NcrId) : IQuery<NcrDetailDto?>;
