using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductAttributes.Queries.GetAttributeValueGroups;

public record GetAttributeValueGroupsQuery : IQuery<IReadOnlyList<string>>;
