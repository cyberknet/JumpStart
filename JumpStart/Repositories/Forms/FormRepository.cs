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
                }
