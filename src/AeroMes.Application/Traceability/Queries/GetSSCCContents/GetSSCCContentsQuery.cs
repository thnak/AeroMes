using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSSCCContents;

public record GetSSCCContentsQuery(string SSCC) : IQuery<SSCCContentsDto>;
