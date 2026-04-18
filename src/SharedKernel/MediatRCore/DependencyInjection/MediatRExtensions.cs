// src/MediatRCore/DependencyInjection/MediatRExtensions.cs
using System.Reflection;
using FluentValidation;
using MediatR;
using MediatRCore.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace MediatRCore.DependencyInjection;

public static class MediatRExtensions
{
    /// <summary>
    /// Add MediatR with all pipeline behaviors
    /// </summary>
    public static IServiceCollection AddMediatRWithBehaviors(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Add MediatR
        services.AddMediatR(cfg =>
        {
            foreach (var assembly in assemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }

            // Register pipeline behaviors in order (first to last executed)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        });

        return services;
    }

    /// <summary>
    /// Add FluentValidation with MediatR integration
    /// </summary>
    public static IServiceCollection AddFluentValidationWithMediatR(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);
        return services;
    }
}
