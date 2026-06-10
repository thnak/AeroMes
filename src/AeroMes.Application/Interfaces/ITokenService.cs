namespace AeroMes.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(string userId, string email, IEnumerable<string> roles, int? workCenterId = null, bool mfaVerified = false);
    string CreateMfaPendingToken(string userId, string email);
}
