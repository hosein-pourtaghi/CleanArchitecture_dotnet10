using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Interfaces.Checklists;
using Application.Common.DTOs;
using Application.Common.DTOs.Shared;
using AutoMapper;
using Domain.Assessments;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AssessmentRepository(ApplicationDbContext _context) : IAssessmentRepository
{ 
    public async Task CreateAssessmentAsync(Assessment assessment)
    {
        // Store current checklist version
        var checklist = await _context.Checklists
            .FirstOrDefaultAsync(c => c.Id == assessment.ChecklistId);

        if (checklist == null)
            throw new KeyNotFoundException("Checklist not found");

        assessment.ChecklistVersion = checklist.Version;
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();
    }


    //public async Task<AssessmentReport> GenerateReportAsync(Guid assessmentId)
    //{
    //    var assessment = await _assessmentRepository.GetByIdAsync(assessmentId);
    //    var historicalChecklist = await _checklistRepository.GetByVersionAsync(
    //        assessment.ChecklistId,
    //        assessment.ChecklistVersion
    //    );

    //    var report = new AssessmentReport
    //    {
    //        AssessmentDate = assessment.AssessmentDate,
    //        TotalScore = assessment.TotalScore,
    //        Questions = new List<QuestionAnswer>()
    //    };

    //    foreach (var answer in assessment.Answers)
    //    {
    //        // Find question in HISTORICAL structure (NOT current)
    //        var question = historicalChecklist.Groups
    //            .SelectMany(g => g.ChecklistQuestions)
    //            .FirstOrDefault(q => q.Id == answer.QuestionId);

    //        if (question == null)
    //            continue; // Shouldn't happen

    //        report.Questions.Add(new QuestionAnswer
    //        {
    //            QuestionText = question.Title,
    //            Answer = answer.AnswerText,
    //            SelectedOptions = question.Options
    //                .Where(o => answer.SelectedOptionIds?.Contains(o.Id) ?? false)
    //                .Select(o => o.Title)
    //                .ToList()
    //        });
    //    }

    //    return report;
    //}
     
    // ===== CRUD OPERATIONS =====
   
    public async Task<Assessment> GetByIdAsync(Guid id)
    {
        return await _context.Assessments
            .Include(a => a.Checklist)
            .Include(a => a.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Assessment with ID {id} not found");
    }

    public async Task<Assessment> CreateAsync(Assessment assessment)
    {
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();
        return assessment;
    }

    public async Task UpdateAsync(Assessment assessment)
    {
        _context.Assessments.Update(assessment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment != null)
        {
            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();
        }
    }

    // ===== REPORT GENERATION =====
    public async Task<PaginatedResult<AssessmentReportItem>> GetAssessmentReportAsync(
        Guid? checklistId = null,
        int? checklistVersion = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 10)
    {
        // Base query (with historical context)
        var query = _context.Assessments
            .Include(a => a.Checklist)
            .Include(a => a.Answers)
                .ThenInclude(a => a.Question)
                    .ThenInclude(q => q.Options)
            .AsNoTracking();

        // Apply filters
        if (checklistId.HasValue)
            query = query.Where(a => a.ChecklistId == checklistId.Value);

        if (checklistVersion.HasValue)
            query = query.Where(a => a.ChecklistVersion == checklistVersion.Value);

        if (fromDate.HasValue)
            query = query.Where(a => a.AssessmentDate >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(a => a.AssessmentDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));

        // Count total items for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var assessments = await query
            .OrderByDescending(a => a.AssessmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Transform to report items
        var reportItems = assessments.Select(a => new AssessmentReportItem
        {
            AssessmentId = a.Id,
            ChecklistTitle = a.Checklist.Title,
            ChecklistVersion = a.ChecklistVersion,
            AssessmentDate = a.AssessmentDate,
            TotalScore = a.TotalScore,
            QuestionCount = a.Answers.Count,
            CompletedQuestionCount = a.Answers.Count(ans =>
                !string.IsNullOrEmpty(ans.AnswerText) ||
                (ans.SelectedOptionIds?.Count > 0)
            ),
            CompletionPercentage = CalculateCompletionPercentage(a)
        }).ToList();

        return new PaginatedResult<AssessmentReportItem>(
            reportItems,
            totalCount,
            page,
            pageSize
        );
    }

    private float CalculateCompletionPercentage(Assessment assessment)
    {
        if (assessment.Answers.Count == 0)
            return 0;

        var completed = assessment.Answers.Count(ans =>
            !string.IsNullOrEmpty(ans.AnswerText) ||
            (ans.SelectedOptionIds?.Count > 0)
        );

        return (float)completed / assessment.Answers.Count * 100;
    }


     
        //public async Task<PaginatedResult<AssessmentReportItem>> GetReportAsync(Filter filter)
        //{
        //    // Simple filtering (using properties)
        //    filter.ChecklistId = new Guid("A");
        //    filter.ChecklistVersion = 1;
        //    filter.FromDate = new DateTime(2023, 1, 1);
        //    filter.ToDate = new DateTime(2023, 12, 31);

        //    // Advanced filtering (using chainable methods)
        //    filter.Or("TotalScore", 90, FilterOperator.GreaterThan)
        //          .And("IsCompleted", true);

        //    return await GetAllAsync<AssessmentReportItem>(filter);
        //}
     
}
