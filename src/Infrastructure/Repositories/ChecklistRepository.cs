using Application.Common.Interfaces.Checklists;
using AutoMapper;
using Domain.Aggregates.Checklists;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Core;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ChecklistRepository : BaseRepository<Checklist>, IChecklistRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ChecklistRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Checklist> GetByIdAsync(Guid id, bool includeGroups = true, bool asNoTracking = true)
    { 
        var query = _context.Checklists
            .Where(c => c.Id == id)
            ;
        if (asNoTracking == true)
        {
            query = query.AsNoTracking();
        }

        if (includeGroups)
        {
            query = query
                .Include(c => c.Groups.Where(g => g.IsShow))
                    .ThenInclude(g => g.Questions.Where(q => q.IsActive))
                    .ThenInclude(q => q.Options.Where(o => o.IsActive))
                .Include(c => c.Groups.Where(g => g.IsShow))
                    .ThenInclude(g => g.Children.Where(g => g.IsShow))
                    .ThenInclude(g => g.Questions.Where(q => q.IsActive))
                    .ThenInclude(q => q.Options.Where(o => o.IsActive));
        }

        return await query.FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Checklist not found");
    }
    public async Task<Checklist> GetByVersionAsync(Guid checklistId, int version)
    {
        return await _context.Checklists
            .Include(c => c.Groups).ThenInclude(g => g.Questions).ThenInclude(q => q.Options)
            .Include(c => c.Groups).ThenInclude(g => g.Children).ThenInclude(g => g.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(c =>
                c.Id == checklistId &&
                c.Version == version
            ) ?? throw new KeyNotFoundException(
                $"Checklist version {version} not found for checklist {checklistId}"
            );
    }

    public async Task<Checklist> UpdateAsync(Guid id, Checklist newStructure)
    {
        // Load checklist with all related entities (not just IsShow=true)
        var checklist = await _context.Checklists
            .Include(c => c.Groups)
                .ThenInclude(g => g.Questions)
                .ThenInclude(q => q.Options)
            .Include(c => c.Groups)
                .ThenInclude(g => g.Children)
                .ThenInclude(g => g.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (checklist == null)
            throw new KeyNotFoundException($"Checklist with ID {id} not found");

        // Store old group IDs before update
        var oldGroupIds = checklist.Groups.Select(g => g.Id).ToHashSet();

        // Apply structure update
        checklist.UpdateStructure(newStructure);

        // Get all groups after update
        var allGroups = checklist.Groups.ToList();
        var newGroupIds = allGroups.Select(g => g.Id).ToHashSet();

        // Find groups that were added (new IDs)
        var addedGroupIds = newGroupIds.Except(oldGroupIds).ToList();

        // Track entities properly
        _context.Entry(checklist).State = EntityState.Modified;

        foreach (var group in allGroups)
        {
            if (addedGroupIds.Contains(group.Id))
            {
                // New group - mark as Added
                _context.Entry(group).State = EntityState.Added;

                // Also mark all children as Added
                MarkGroupAndChildrenAsAdded(group);
            }
            else
            {
                // Existing group - check if it was modified
                var entry = _context.Entry(group);
                if (entry.State == EntityState.Unchanged && group.IsShow == false)
                {
                    // Old group that was hidden - mark as Modified
                    entry.State = EntityState.Modified;
                }
            }
        }

        await _context.SaveChangesAsync();

        return checklist;
        //return await GetByIdAsync(id, true, false);
    }

    private void MarkGroupAndChildrenAsAdded(ChecklistGroup group)
    { 
        _context.Entry(group).State = EntityState.Added;

        foreach (var question in group.Questions)
        {
            _context.Entry(question).State = EntityState.Added;

            if (question.Options != null)
            {
                foreach (var option in question.Options)
                {
                    _context.Entry(option).State = EntityState.Added;
                }
            }
        }

        foreach (var child in group.Children)
        {
            MarkGroupAndChildrenAsAdded(child);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var checklist = await _context.Checklists.FindAsync(id);
        if (checklist != null)
        {
            _context.Checklists.Remove(checklist);
            await _context.SaveChangesAsync();
        }
    }
}
