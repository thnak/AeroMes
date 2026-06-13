using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.EngChanges.Commands.SubmitEngChangeForReview;

public record SubmitEngChangeForReviewCommand(string EcNumber, string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
