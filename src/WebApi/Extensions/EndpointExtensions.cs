using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WebApi.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddAuthorization(this IServiceCollection services, string policy, string permission)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(policy, builder => builder.RequireAuthenticatedUser());
        });

        return services;
    }
}
