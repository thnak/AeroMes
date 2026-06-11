using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMes.Api.Controllers;
using AeroMes.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Constants;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ForgotPasswordRequest))]
[JsonSerializable(typeof(ResetPasswordRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(MfaPendingResult))]
[JsonSerializable(typeof(RefreshTokenResult))]
[JsonSerializable(typeof(UserProfileResult))]
[JsonSerializable(typeof(UpdateMeRequest))]
[JsonSerializable(typeof(ChangePasswordRequest))]
[JsonSerializable(typeof(MfaEnforcementMiddleware.MfaVerifyRequiredResponse))]
[JsonSerializable(typeof(ForcePasswordChangeMiddleware.ForcePasswordChangeResponse))]
public partial class ApiJsonContext : JsonSerializerContext
{
    
}