// Copyright ©2026 Scott Blomfield
/*
 *  This program is free software: you can redistribute it and/or modify it under the terms of the
 *  GNU General Public License as published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 *  even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with this program. If not,
 *  see <https://www.gnu.org/licenses/>. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JumpStart.Api.DTOs.Forms;
using JumpStart.Data.Advanced.Auditing;
using JumpStart.Forms;
using Microsoft.EntityFrameworkCore;

namespace JumpStart.Repositories.Forms;

/// <summary>
/// Repository implementation for managing <see cref="Form"/> entities.
/// </summary>
/// <remarks>
/// <para>
/// This repository provides data access for forms, including specialized queries
/// for active forms, form statistics, and eager-loading of related questions and options.
/// </para>
/// <para>
/// All methods use async/await for optimal performance and follow JumpStart's
/// repository patterns including audit tracking through <see cref="ISimpleUserContext"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register in DI container
/// services.AddScoped&lt;IFormRepository, FormRepository&gt;();
/// 
/// // Use in a service
/// public class FormDisplayService
/// {
///     private readonly IFormRepository _repository;
///     
///     public FormDisplayService(IFormRepository repository)
///     {
///         _repository = repository;
///     }
///     
///     public async Task&lt;Form?&gt; GetFormForUserAsync(Guid formId)
///     {
///         var form = await _repository.GetFormWithQuestionsAsync(formId);
///         
///         if (form == null || !form.IsActive)
///             return null;
///             
///         return form;
///     }
/// }
/// </code>
/// </example>
public class FormRepository(DbContext context, ISimpleUserContext? userContext) 
    : SimpleRepository<Form>(context, userContext), IFormRepository
{
    /// <summary>
    /// Retrieves all active forms.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a list of forms where <see cref="Form.IsActive"/> is true,
    /// ordered by name.
    /// </returns>
    /// <remarks>
    /// This query filters forms to only include those currently accepting responses.
    /// Does not include soft-deleted forms (IsDeleted = false).
    /// </remarks>
    public async Task<IList<Form>> GetActiveFormsAsync()
    {
        return await _dbSet
            .Where(f => f.IsActive && f.DeletedOn == null)
            .OrderBy(f => f.Name)
            .ToListAsync();
    }
    
    /// <summary>
    /// Retrieves a form with all related questions and their options.
    /// </summary>
    /// <param name="formId">The unique identifier of the form.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the form with all questions and options loaded,
    /// or null if the form is not found or soft-deleted.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses eager loading (.Include) to retrieve all related data in a single query,
    /// optimizing performance when displaying forms.
    /// </para>
    /// <para>
    /// The loaded data includes:
    /// - All questions for the form
    /// - All options for each question
    /// </para>
    /// <para>
    /// Questions and options should be ordered by DisplayOrder in the consuming code.
    /// </para>
    /// </remarks>
    public async Task<Form?> GetFormWithQuestionsAsync(Guid formId)
    {
        return await _dbSet
            .Include(f => f.Questions.OrderBy(q => q.DisplayOrder))
                .ThenInclude(q => q.QuestionType)
            .Include(f => f.Questions)
                .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
            .Where(f => f.DeletedOn == null)
            .FirstOrDefaultAsync(f => f.Id == formId);
    }
    
    /// <summary>
    /// Gets the total count of responses submitted for a specific form.
    /// </summary>
    /// <param name="formId">The unique identifier of the form.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the count of all responses (complete and incomplete).
    /// </returns>
    /// <remarks>
    /// This count includes both complete and incomplete responses.
    /// Does not include responses for soft-deleted forms.
    /// </remarks>
    public async Task<int> GetFormResponseCountAsync(Guid formId)
    {
        return await _context.Set<FormResponse>()
            .Where(fr => fr.FormId == formId && fr.DeletedOn == null)
            .CountAsync();
    }
    
    /// <summary>
    /// Gets the count of complete responses submitted for a specific form.
    /// </summary>
    /// <param name="formId">The unique identifier of the form.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the count of complete responses
    /// (where <see cref="FormResponse.IsComplete"/> is true).
    /// </returns>
    /// <remarks>
        /// Only counts responses marked as complete. Useful for calculating
        /// form completion rates and analyzing successful submissions.
        /// </remarks>
        public async Task<int> GetCompleteResponseCountAsync(Guid formId)
        {
            return await _context.Set<FormResponse>()
                .Where(fr => fr.FormId == formId && fr.IsComplete && fr.DeletedOn == null)
                .CountAsync();
        }

        /// <summary>
        /// Gets a question type by its code.
        /// </summary>
        public async Task<QuestionType?> GetQuestionTypeByCodeAsync(string code)
        {
            return await _context.Set<QuestionType>()
                .FirstOrDefaultAsync(qt => qt.Code == code);
        }

            /// <summary>
            /// Gets all available question types, ordered by display order.
            /// </summary>
            public async Task<IList<QuestionType>> GetAllQuestionTypesAsync()
            {
                return await _context.Set<QuestionType>()
                    .OrderBy(qt => qt.DisplayOrder)
                    .ToListAsync();
            }

            /// <summary>
            /// Gets a question type by its ID.
            /// </summary>
            public async Task<QuestionType?> GetQuestionTypeByIdAsync(Guid id)
            {
                return await _context.Set<QuestionType>()
                    .FirstOrDefaultAsync(qt => qt.Id == id);
            }

            /// <summary>
            /// Creates a new question type.
            /// </summary>
            public async Task<QuestionType> CreateQuestionTypeAsync(QuestionType questionType)
            {
                await _context.Set<QuestionType>().AddAsync(questionType);
                await _context.SaveChangesAsync();
                return questionType;
            }

            /// <summary>
            /// Updates an existing question type.
            /// </summary>
            public async Task UpdateQuestionTypeAsync(QuestionType questionType)
            {
                _context.Set<QuestionType>().Update(questionType);
                await _context.SaveChangesAsync();
            }

            /// <summary>
            /// Deletes a question type.
            /// </summary>
            public async Task DeleteQuestionTypeAsync(Guid id)
            {
                var questionType = await GetQuestionTypeByIdAsync(id);
                if (questionType != null)
                {
                    _context.Set<QuestionType>().Remove(questionType);
                    await _context.SaveChangesAsync();
                }
            }

            /// <summary>
            /// Saves a form response with all question responses and selected options.
            /// </summary>
            public async Task<FormResponse> SaveFormResponseAsync(FormResponse formResponse)
            {
                // Add the form response (cascade will handle question responses)
                await _context.Set<FormResponse>().AddAsync(formResponse);
                await _context.SaveChangesAsync();

                return formResponse;
            }

                    /// <summary>
                    /// Gets a form response by ID with all related data loaded.
                    /// </summary>
                    public async Task<FormResponse?> GetFormResponseAsync(Guid responseId)
                    {
                        return await _context.Set<FormResponse>()
                            .Include(fr => fr.Form)
                            .Include(fr => fr.Answers)
                                .ThenInclude(qr => qr.Question)
                            .Include(fr => fr.Answers)
                                .ThenInclude(qr => qr.SelectedOptions)
                                    .ThenInclude(so => so.QuestionOption)
                            .FirstOrDefaultAsync(fr => fr.Id == responseId);
                    }

                    /// <summary>
                    /// Deletes all responses for a specific form (hard delete).
                    /// </summary>
                    public async Task<int> DeleteAllFormResponsesAsync(Guid formId)
                    {
                        var responses = await _context.Set<FormResponse>()
                            .Where(fr => fr.FormId == formId)
                            .ToListAsync();

                        var count = responses.Count;

                        if (count > 0)
                        {
                            _context.Set<FormResponse>().RemoveRange(responses);
                            await _context.SaveChangesAsync();
                        }

                        return count;
                    }
                    
                    /// <summary>
                    /// Updates a form including its questions and options.
                    /// </summary>
                    public async Task UpdateFormWithQuestionsAsync(Guid formId, UpdateFormDto updateDto)
                    {
                        var existingForm = await GetFormWithQuestionsAsync(formId);
                        if (existingForm == null)
                        {
                            throw new InvalidOperationException($"Form {formId} not found");
                        }
                        
                        // Update form properties
                        existingForm.Name = updateDto.Name;
                        existingForm.Description = updateDto.Description;
                        existingForm.IsActive = updateDto.IsActive;
                        existingForm.AllowMultipleResponses = updateDto.AllowMultipleResponses;
                        existingForm.AllowAnonymous = updateDto.AllowAnonymous;
                        
                        // Update modification audit fields
                        if (existingForm is IModifiable<Guid> modifiableForm)
                        {
                            modifiableForm.ModifiedOn = DateTimeOffset.UtcNow;
                            if (_userContext != null)
                            {
                                var userId = await _userContext.GetCurrentUserIdAsync();
                                if (userId.HasValue)
                                {
                                    modifiableForm.ModifiedById = userId.Value;
                                }
                            }
                        }
                        
                        // Synchronize questions
                        await SynchronizeQuestionsAsync(existingForm, updateDto.Questions);
                        
                        // Save all changes
                        await _context.SaveChangesAsync();
                    }
                    
                    /// <summary>
                    /// Synchronizes the questions collection for a form during update.
                    /// </summary>
                    private async Task SynchronizeQuestionsAsync(Form form, List<UpdateQuestionDto> questionDtos)
                    {
                        var existingQuestions = form.Questions.ToList();
                        var incomingQuestionIds = questionDtos.Where(q => q.Id.HasValue).Select(q => q.Id.Value).ToHashSet();
                        
                        // Remove questions that are no longer in the update
                        var questionsToRemove = existingQuestions.Where(q => !incomingQuestionIds.Contains(q.Id)).ToList();
                        foreach (var question in questionsToRemove)
                        {
                            form.Questions.Remove(question);
                        }
                        
                        // Add or update questions
                        foreach (var questionDto in questionDtos)
                        {
                            if (questionDto.Id.HasValue)
                            {
                                // Update existing question
                                var existingQuestion = existingQuestions.FirstOrDefault(q => q.Id == questionDto.Id.Value);
                                if (existingQuestion != null)
                                {
                                    existingQuestion.QuestionText = questionDto.QuestionText;
                                    existingQuestion.HelpText = questionDto.HelpText;
                                    existingQuestion.QuestionTypeId = questionDto.QuestionTypeId;
                                    existingQuestion.IsRequired = questionDto.IsRequired;
                                    existingQuestion.MinimumValue = questionDto.MinimumValue;
                                    existingQuestion.MaximumValue = questionDto.MaximumValue;
                                    existingQuestion.DisplayOrder = questionDto.DisplayOrder;
                                    
                                    // Synchronize options
                                    SynchronizeOptionsAsync(existingQuestion, questionDto.Options);
                                }
                            }
                            else
                            {
                                // Add new question - verify question type exists
                                var questionTypeExists = await GetQuestionTypeByIdAsync(questionDto.QuestionTypeId);
                                if (questionTypeExists == null)
                                {
                                    throw new InvalidOperationException($"Question type {questionDto.QuestionTypeId} not found");
                                }
                                
                                var newQuestion = new Question
                                {
                                    Id = Guid.NewGuid(),
                                    FormId = form.Id,
                                    QuestionText = questionDto.QuestionText,
                                    HelpText = questionDto.HelpText,
                                    QuestionTypeId = questionDto.QuestionTypeId,
                                    IsRequired = questionDto.IsRequired,
                                    MinimumValue = questionDto.MinimumValue,
                                    MaximumValue = questionDto.MaximumValue,
                                    DisplayOrder = questionDto.DisplayOrder
                                };
                                
                                // Add options for new question
                                foreach (var optionDto in questionDto.Options)
                                {
                                    newQuestion.Options.Add(new QuestionOption
                                    {
                                        Id = Guid.NewGuid(),
                                        QuestionId = newQuestion.Id,
                                        OptionText = optionDto.OptionText,
                                        OptionValue = optionDto.OptionValue,
                                        DisplayOrder = optionDto.DisplayOrder
                                    });
                                }
                                
                                // Explicitly mark as Added to avoid confusion
                                form.Questions.Add(newQuestion);
                                _context.Entry(newQuestion).State = EntityState.Added;
                                
                                // Mark all options as Added too
                                foreach (var option in newQuestion.Options)
                                {
                                    _context.Entry(option).State = EntityState.Added;
                                }
                            }
                        }
                    }
                    
                    /// <summary>
                    /// Synchronizes the options collection for a question during update.
                    /// </summary>
                    private void SynchronizeOptionsAsync(Question question, List<UpdateQuestionOptionDto> optionDtos)
                    {
                        var existingOptions = question.Options.ToList();
                        var incomingOptionIds = optionDtos.Where(o => o.Id.HasValue).Select(o => o.Id.Value).ToHashSet();
                        
                        // Remove options that are no longer in the update
                        var optionsToRemove = existingOptions.Where(o => !incomingOptionIds.Contains(o.Id)).ToList();
                        foreach (var option in optionsToRemove)
                        {
                            question.Options.Remove(option);
                        }
                        
                        // Add or update options
                        foreach (var optionDto in optionDtos)
                        {
                            if (optionDto.Id.HasValue)
                            {
                                // Update existing option
                                var existingOption = existingOptions.FirstOrDefault(o => o.Id == optionDto.Id.Value);
                                if (existingOption != null)
                                {
                                    existingOption.OptionText = optionDto.OptionText;
                                    existingOption.OptionValue = optionDto.OptionValue;
                                    existingOption.DisplayOrder = optionDto.DisplayOrder;
                                }
                            }
                            else
                            {
                                // Add new option
                                var newOption = new QuestionOption
                                {
                                    Id = Guid.NewGuid(),
                                    QuestionId = question.Id,
                                    OptionText = optionDto.OptionText,
                                    OptionValue = optionDto.OptionValue,
                                    DisplayOrder = optionDto.DisplayOrder
                                };
                                question.Options.Add(newOption);
                                _context.Entry(newOption).State = EntityState.Added;
                            }
                        }
                    }
                    
                    /// <summary>
                    /// Saves all pending changes to the database.
                    /// </summary>
                    public async Task SaveChangesAsync()
                    {
                        await _context.SaveChangesAsync();
                    }
                }
