using AeroMes.Application.Common.Behaviors;
using FluentValidation;
using LiteBus.Commands;
using LiteBus.Events;
using LiteBus.Extensions.Microsoft.DependencyInjection;
using LiteBus.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace AeroMes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddLiteBus(liteBus =>
        {
            liteBus.AddCommandModule(module =>
            {
                module.Register(typeof(FluentValidationPreHandler<>));
                module.RegisterFromAssembly(assembly);
            });
            liteBus.AddQueryModule(module =>
            {
                module.RegisterFromAssembly(assembly);
            });
            liteBus.AddEventModule(module =>
            {
                module.RegisterFromAssembly(assembly);
            });
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
