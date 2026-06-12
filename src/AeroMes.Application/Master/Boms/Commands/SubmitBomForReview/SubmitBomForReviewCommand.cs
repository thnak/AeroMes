using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Boms.Commands.SubmitBomForReview;

public record SubmitBomForReviewCommand(
    string ProductCode,
    string Version,
    string? UpdatedBy) : ICommand;
