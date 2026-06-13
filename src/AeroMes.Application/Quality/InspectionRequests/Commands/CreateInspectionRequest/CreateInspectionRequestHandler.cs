using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionRequests.Commands.CreateInspectionRequest;

public class CreateInspectionRequestHandler(
    IQualityInspectionRequestRepository repository,
    IValidator<CreateInspectionRequestCommand> validator)
    : ICommandHandler<CreateInspectionRequestCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateInspectionRequestCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repository.RequestNumberExistsAsync(command.RequestNumber, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["requestNumber"] = ["Số phiếu yêu cầu đã tồn tại."]
            });

        try
        {
            var request = QualityInspectionRequest.Create(
                command.RequestNumber, command.RequestDate, command.InspectionPurpose,
                command.RequesterName, command.RequestingDepartment, command.RecipientPerson,
                command.InspectionDeadline, command.CreatedBy);

            var id = await repository.AddAsync(request, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
