using SharedKernel.DynamicCrud;


namespace SharedApi.DynamicCrud;


public sealed class DynamicCrudOptions
{

    private readonly List<Type> _entities = [];


    public IReadOnlyCollection<Type> Entities
        => _entities;



    public void Register<TEntity>()
        where TEntity : class, IDynamicCrudEntity
    {
        _entities.Add(typeof(TEntity));
    }

}
