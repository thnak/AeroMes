namespace AeroMes.Application.Localization;

// Marker class for IStringLocalizer<SharedResources>.
// Resource files:
//   Localization/SharedResources.resx        (vi — default)
//   Localization/SharedResources.en-US.resx  (en-US)
// Per-use-case resources live alongside the handler:
//   {Context}/{Commands|Queries}/{UseCase}/{UseCase}Messages.resx  (vi)
//   {Context}/{Commands|Queries}/{UseCase}/{UseCase}Messages.en-US.resx
public sealed class SharedResources { }
