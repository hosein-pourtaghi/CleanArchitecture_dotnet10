using System.Reflection;
using Application.Common.DynamicCrud.Handlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.DynamicCrud;


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



        //foreach (var entity in entities)
        //{ 
        //    services.AddTransient(
        //        typeof(IRequestHandler<,>),
        //        typeof(CreateDynamicCrudCommandHandler<>)
        //            .MakeGenericType(entity));


        //    services.AddTransient(
        //        typeof(IRequestHandler<,>),
        //        typeof(UpdateDynamicCrudCommandHandler<>)
        //            .MakeGenericType(entity));


        //    services.AddTransient(
        //        typeof(IRequestHandler<,>),
        //        typeof(DeleteDynamicCrudCommandHandler<>)
        //            .MakeGenericType(entity));


        //    services.AddTransient(
        //        typeof(IRequestHandler<,>),
        //        typeof(GetDynamicCrudByIdQueryHandler<>)
        //            .MakeGenericType(entity));


        //    services.AddTransient(
        //        typeof(IRequestHandler<,>),
        //        typeof(GetDynamicCrudListQueryHandler<>)
        //            .MakeGenericType(entity));
        //}



        foreach (var entity in entities)
        {
            RegisterHandler(
                services,
                typeof(CreateDynamicCrudCommandHandler<>),
                entity);


            RegisterHandler(
                services,
                typeof(UpdateDynamicCrudCommandHandler<>),
                entity);


            RegisterHandler(
                services,
                typeof(DeleteDynamicCrudCommandHandler<>),
                entity);


            RegisterHandler(
                services,
                typeof(GetDynamicCrudByIdQueryHandler<>),
                entity);


            RegisterHandler(
                services,
                typeof(GetDynamicCrudListQueryHandler<>),
                entity);
        }






        return services;
    }





    private static void RegisterHandler(
        IServiceCollection services,
        Type openHandlerType,
        Type entityType)
    {
        var closedHandlerType =
            openHandlerType.MakeGenericType(entityType);


        var handlerInterface =
            closedHandlerType
            .GetInterfaces()
            .First(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition()
                == typeof(IRequestHandler<,>));


        services.AddTransient(
            handlerInterface,
            closedHandlerType);
    }




}
