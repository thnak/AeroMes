using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetBundleLocation;

public record GetBundleLocationQuery(string BundleBarcode) : IQuery<BundleLocationDto?>;
