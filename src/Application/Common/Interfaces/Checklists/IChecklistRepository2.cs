using Application.Common.Data;
using Domain.Checklists;

namespace Application.Common.Interfaces.Checklists;
 
///// <summary>
///// Checklist repository interface - Application layer
///// No dependency on Infrastructure (DbContext)
///// </summary>
//public interface IChecklistRepository2
//{
//    #region Basic Operations
//    Task<Checklist?> GetByIdAsync(Guid id, bool includeGroups = true, bool asNoTracking = false);
//    Task<Checklist?> GetByVersionAsync(Guid checklistId, int version);
//    Task<Checklist?> GetByIdWithAllVersionsAsync(Guid id);
//    Task<List<Checklist>> GetAllActiveAsync(CancellationToken cancellationToken = default);
//    Task<Checklist> AddAsync(Checklist entity, CancellationToken cancellationToken = default);
//    #endregion

//    #region Simple Update (No Versioning)
//    Task<Checklist> UpdateSimpleAsync(Guid id, Checklist newStructure);
//    Task<Checklist> UpdateWithConfigAsync(Guid id, Checklist newStructure);
//    #endregion

//    #region Versioning Operations
//    Task<Checklist> UpdateWithVersioningAsync(Guid id, Checklist newStructure);
//    Task<Checklist> CreateNewVersionAsync(Guid id);
//    Task<List<Checklist>> GetAllVersionsAsync(Guid checklistId);
//    #endregion

//    #region Group Operations
//    Task<ChecklistGroup?> GetGroupByIdAsync(Guid groupId, CancellationToken cancellationToken = default);
//    Task<Checklist> AddGroupAsync(Guid checklistId, ChecklistGroup group, CancellationToken cancellationToken = default);
//    Task<Checklist> UpdateGroupAsync(Guid groupId, ChecklistGroup group, CancellationToken cancellationToken = default);
//    Task<Checklist> DeleteGroupAsync(Guid checklistId, Guid groupId, CancellationToken cancellationToken = default);
//    #endregion

//    #region Question Operations
//    Task<ChecklistQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken = default);
//    Task<Checklist> AddQuestionAsync(Guid groupId, ChecklistQuestion question, CancellationToken cancellationToken = default);
//    Task<Checklist> UpdateQuestionAsync(Guid questionId, ChecklistQuestion question, CancellationToken cancellationToken = default);
//    Task<Checklist> DeleteQuestionAsync(Guid groupId, Guid questionId, CancellationToken cancellationToken = default);
//    #endregion

//    #region Generic CRUD (from base)
//    Task<List<Checklist>> GetAllAsync(CancellationToken cancellationToken = default);
//    Task UpdateAsync(Checklist entity, CancellationToken cancellationToken = default);
//    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
//    #endregion
//}
