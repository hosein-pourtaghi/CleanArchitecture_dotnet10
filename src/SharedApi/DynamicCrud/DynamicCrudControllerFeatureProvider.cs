using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace SharedApi.DynamicCrud;

internal sealed class DynamicCrudControllerFeatureProvider
    : IApplicationFeatureProvider<ControllerFeature>
{
    private readonly IReadOnlyCollection<Type> _controllerTypes;


    public DynamicCrudControllerFeatureProvider(
        IEnumerable<Type> controllerTypes)
    {
        _controllerTypes = controllerTypes.ToList();
    }


    public void PopulateFeature(
        IEnumerable<ApplicationPart> parts,
        ControllerFeature feature)
    {
        foreach (var controllerType in _controllerTypes)
        {
            var controllerInfo =
                controllerType.GetTypeInfo();


            if (!feature.Controllers.Contains(controllerInfo))
            {
                feature.Controllers.Add(controllerInfo);
            }
        }
    }
}
