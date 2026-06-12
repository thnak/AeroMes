using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.EngChanges.Commands.SubmitEngChangeForReview;

public record SubmitEngChangeForReviewCommand(string EcNumber, string? UpdatedBy) : ICommand;
