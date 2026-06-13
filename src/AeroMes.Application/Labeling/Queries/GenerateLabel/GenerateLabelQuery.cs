using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Labeling.Queries.GenerateLabel;

public sealed record GenerateLabelQuery(
    string ContentType,
    string EntityId,
    string Format,
    string Output) : IQuery<GenerateLabelResult>;

public sealed record GenerateLabelResult(byte[] Data, string ContentType, string FileName);
