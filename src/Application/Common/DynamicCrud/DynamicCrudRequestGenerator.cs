using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection;


namespace Application.Common.DynamicCrud;


internal static class DynamicCrudRequestGenerator
{

    private static readonly AssemblyBuilder Assembly =
        System.Reflection.Emit.AssemblyBuilder
        .DefineDynamicAssembly(
            new AssemblyName(
                "DynamicCrud.Generated"),
            AssemblyBuilderAccess.Run);



    private static readonly ModuleBuilder Module =
        Assembly.DefineDynamicModule(
            "DynamicCrudModule");



    public static void Register(
        Type entityType,
        IServiceCollection services)
    {

        CreateCommand(entityType);
        GetQuery(entityType);

    }



    private static Type CreateCommand(
        Type entityType)
    {

        var name =
            $"Create{entityType.Name}Command";


        var typeBuilder =
            Module.DefineType(
                name,
                TypeAttributes.Public |
                TypeAttributes.Class);



        typeBuilder
            .AddInterfaceImplementation(
                typeof(IRequest<>)
                .MakeGenericType(entityType));


        return typeBuilder.CreateType()!;
    }

}
