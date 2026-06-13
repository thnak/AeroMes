namespace AeroMes.Application.Traceability.Services;

public interface IESignatureService
{
    /// <summary>
    /// Validates the e-signature token for the given user.
    /// Returns the signature reference to store on the record.
    /// Throws if the token is invalid or expired.
    /// </summary>
    Task<string> ValidateAndRecordAsync(
        string userId,
        string signatureToken,
        string meaningStatement,
        CancellationToken ct = default);
}
