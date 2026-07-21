using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.DynamicCrud;
using Application.Common.DynamicCrud.Handlers;


namespace Application.Common.DynamicCrud;


public static class DynamicCrudApplicationExtensions
{

    public static IServiceCollection AddDynamicCrudApplication(
        this IServiceCollection services,
        Assembly domainAssembly)
    {

        var entities =
            domainAssembly.GetTypes()
            .Where(x =>
                typeof(IDynamicCrudEntity)
                .IsAssignableFrom(x)
                &&
                x.IsClass
                &&
                !x.IsAbstract);



        foreach (var entity in entities)
        {

            services.AddTransient(
                typeof(IRequestHandler<,>),
                typeof(CreateDynamicCrudCommandHandler<>)
                    .MakeGenericType(entity));


            services.AddTransient(
                typeof(IRequestHandler<,>),
                typeof(UpdateDynamicCrudCommandHandler<>)
                    .MakeGenericType(entity));


            services.AddTransient(
                typeof(IRequestHandler<,>),
                typeof(DeleteDynamicCrudCommandHandler<>)
                    .MakeGenericType(entity));


            services.AddTransient(
                typeof(IRequestHandler<,>),
                typeof(GetDynamicCrudByIdQueryHandler<>)
                    .MakeGenericType(entity));


            services.AddTransient(
                typeof(IRequestHandler<,>),
                typeof(GetDynamicCrudListQueryHandler<>)
                    .MakeGenericType(entity));
        }


        return services;
    }
}
