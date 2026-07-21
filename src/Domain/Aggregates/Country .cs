using SharedKernel.DynamicCrud;

namespace Domain.Aggregates;

public class Country : IDynamicCrudEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
