using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.DynamicCrud;

namespace Application.Common.DynamicCrud;


public static class DynamicCrudApplicationExtensions
{
    public static IServiceCollection AddDynamicCrudApplication(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {

        var entities = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x =>
                typeof(IDynamicCrudEntity)
                    .IsAssignableFrom(x)
                &&
                x.IsClass
                &&
                !x.IsAbstract)
            .ToList();



        foreach (var entity in entities)
        {
            DynamicCrudRequestGenerator
                .Register(entity, services);


            DynamicCrudHandlerGenerator
                .Register(entity, services);
        }


        return services;
    }
}
