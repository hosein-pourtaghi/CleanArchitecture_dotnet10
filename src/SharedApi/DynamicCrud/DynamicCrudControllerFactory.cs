using System.Reflection;
using System.Reflection.Emit;


namespace SharedApi.DynamicCrud;


internal static class DynamicCrudControllerFactory
{


    public static Type Create(Type entityType)
    {

        var assembly =
            AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(
                    $"DynamicCrud_{entityType.Name}"),
                AssemblyBuilderAccess.Run);



        var module =
            assembly.DefineDynamicModule(
                "Main");



        var controllerType =
            typeof(Controllers.DynamicCrudController<>)
            .MakeGenericType(entityType);



        var builder =
            module.DefineType(
                $"{entityType.Name}Controller",
                TypeAttributes.Public,
                controllerType);



        return builder.CreateType()!;

    }

}
