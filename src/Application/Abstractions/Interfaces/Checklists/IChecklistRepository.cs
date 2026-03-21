using Domain.Checklists;

namespace Application.Abstractions.Interfaces.Checklists;

public interface IChecklistRepository
{
    Task<Checklist> GetByIdAsync(Guid id, bool includeGroups = true);
    Task<Checklist> GetByVersionAsync(Guid checklistId, int version);

    Task<Checklist> AddAsync(Checklist checklist);
    Task<Checklist> UpdateAsync(Guid id, Checklist newStructure);

    Task DeleteAsync(Guid id);



}
