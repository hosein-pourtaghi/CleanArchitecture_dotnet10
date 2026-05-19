using SharedKernel.Messaging;

namespace SharedKernel.BaseEntities;


public abstract class Entity
{
    public Guid Id { get; set; } /// change this to private

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public Guid? CreatedById { get; protected set; }
    public Guid? UpdatedById { get; protected set; }

    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public Guid? DeletedById { get; protected set; }


    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


    public void SetCreatedBy(Guid? userId)
    {
        CreatedById = userId;
    }
    public void SetUpdatedBy(Guid? userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = userId;
    }
    public void SoftDelete(Guid? userId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedById = userId;
    }
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedById = null;
    }

    public void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }


}
