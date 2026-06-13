using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterials;

public record GetSubstituteMaterialsQuery(
    string? PrimaryMaterialCode,
    bool? ActiveOnly) : IQuery<IReadOnlyList<SubstituteMaterialDto>>;
