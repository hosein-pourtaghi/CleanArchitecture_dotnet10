using System.Reflection;
using Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    { 
    services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register MediatR with all handlers from this assembly
        services.AddMediatR(config =>
        {
            //config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Register custom pipeline behaviors
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // Add FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly()); // other way of get assembly

        // Register domain event handlers
        services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
