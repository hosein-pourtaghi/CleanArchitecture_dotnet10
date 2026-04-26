// src/Application/DependencyInjection.cs
// ============================================================
// PURPOSE: Application layer dependency injection configuration
// ============================================================

using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using SharedKernel.MediatRCore.Behaviors;

namespace Application;

/// <summary>
/// Extension methods for configuring application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application layer services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register AutoMapper profiles from this assembly
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register MediatR with all handlers from this assembly
        services.AddMediatR(config =>
        {
            // Register all services from the Application assembly
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // Register custom pipeline behaviors
            // Validation behavior runs before handlers
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));

        });

        // Add FluentValidation validators from this assembly
        // includeInternalTypes: true - includes internal validators
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        // Alternative way to register validators:
        // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register domain event handlers
        // Scans for classes implementing IDomainEventHandler<> interfaces
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
