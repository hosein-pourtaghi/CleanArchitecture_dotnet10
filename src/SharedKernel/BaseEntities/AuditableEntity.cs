using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.BaseEntities;
 
public abstract class AuditableEntity : Entity
{
    // todo:make them private and  [NotMapped]  annotation

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public string? CreatedById { get; protected set; }
    public string? UpdatedById { get; protected set; }
     

    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedById { get; protected set; }


    public void SetCreatedBy(string? userId)
    {
        CreatedById = userId;
    }

    public void SetUpdatedBy(string? userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = userId;
    }

    public void SoftDelete(string? userId)
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

}
