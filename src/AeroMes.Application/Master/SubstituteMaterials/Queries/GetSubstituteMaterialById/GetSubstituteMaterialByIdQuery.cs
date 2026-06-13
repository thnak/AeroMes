using AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterials;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.SubstituteMaterials.Queries.GetSubstituteMaterialById;

public record GetSubstituteMaterialByIdQuery(int SubstituteId) : IQuery<SubstituteMaterialDto?>;
