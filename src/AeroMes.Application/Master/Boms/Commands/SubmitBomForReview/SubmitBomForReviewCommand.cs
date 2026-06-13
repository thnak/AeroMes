using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.Boms.Commands.SubmitBomForReview;

public record SubmitBomForReviewCommand(
    string ProductCode,
    string Version,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
