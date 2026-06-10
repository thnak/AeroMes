using AeroMes.Domain.Settings;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Settings.Queries.GetSystemOptions;

public record GetSystemOptionsQuery : IQuery<SystemOptions>;
