using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.DynamicCrud;

namespace SharedApi.DynamicCrud;

public static class DynamicCrudExtensions
{
    public static IServiceCollection AddDynamicCrud(
        this IServiceCollection services)
    {
        var domainAssembly = FindAssembly("Domain");


        if (domainAssembly is null)
        {
            throw new InvalidOperationException(
                "Could not find Domain assembly.");
        }


        var entities = domainAssembly
            .GetTypes()
            .Where(x =>
                typeof(IDynamicCrudEntity)
                    .IsAssignableFrom(x)
                &&
                x.IsClass
                &&
                !x.IsAbstract)
            .ToList();


        services.AddSingleton<IReadOnlyCollection<Type>>(entities);


        services
            .AddControllers()
            .ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(
                    new DynamicCrudControllerFeatureProvider(
                        entities));
            });
        //    services
        //.Configure<Microsoft.AspNetCore.Mvc.ApplicationPartManager>(
        //    manager =>
        //    {
        //        manager.FeatureProviders.Add(
        //            new DynamicCrudControllerFeatureProvider(
        //                entities));
        //    });

        return services;
    }



    private static Assembly? FindAssembly(string assemblyName)
    {
        var loadedAssemblies =
            AppDomain.CurrentDomain
                .GetAssemblies();


        return loadedAssemblies
            .FirstOrDefault(x =>
                x.GetName()
                 .Name?
                 .Equals(
                     assemblyName,
                     StringComparison.OrdinalIgnoreCase)
                 == true);
    }


    //private static Assembly? FindAssembly(string name)
    //{
    //    var assemblies =
    //        Assembly.GetExecutingAssembly()
    //            .GetReferencedAssemblies();


    //    var domain =
    //        assemblies.FirstOrDefault(x =>
    //            x.Name?.Equals(
    //                name,
    //                StringComparison.OrdinalIgnoreCase)
    //            == true);


    //    if (domain == null)
    //        return null;


    //    return Assembly.Load(domain);
    //}

    //private static Assembly? FindDomainAssembly()
    //{
    //    var infrastructure =
    //        typeof(DynamicCrudExtensions).Assembly
    //            .GetReferencedAssemblies();


    //    var application =
    //        infrastructure
    //            .FirstOrDefault(x =>
    //                x.Name == "Application");


    //    if (application == null)
    //        return null;


    //    var applicationAssembly =
    //        Assembly.Load(application);


    //    var domainReference =
    //        applicationAssembly
    //            .GetReferencedAssemblies()
    //            .FirstOrDefault(x =>
    //                x.Name == "Domain");


    //    return domainReference == null
    //        ? null
    //        : Assembly.Load(domainReference);
    //}




}
