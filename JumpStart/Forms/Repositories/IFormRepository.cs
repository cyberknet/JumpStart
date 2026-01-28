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
using System.Threading.Tasks;
using JumpStart.Forms;
using JumpStart.Forms.DTOs;
using JumpStart.Repositories;

namespace JumpStart.Forms.Repositories;

/// <summary>
/// Repository interface for managing <see cref="Form"/> entities.
/// Provides data access methods for forms and their related questions and responses.
/// </summary>
/// <remarks>
/// <para>
/// This repository extends the basic CRUD operations with form-specific queries
/// including active form retrieval, form statistics, and response management.
/// </para>
/// <para>
/// <strong>Common Use Cases:</strong>
/// - Retrieve active forms for display to users
/// - Get form with all questions and options for rendering
/// - Query form statistics and response counts
/// - Manage form lifecycle (create, update, activate/deactivate, delete)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class FormService
/// {
///     private readonly IFormRepository _formRepository;
///     
///     public FormService(IFormRepository formRepository)
///     {
///         _formRepository = formRepository;
///     }
///     
///     public async Task&lt;IList&lt;Form&gt;&gt; GetUserFormsAsync()
///     {
///         // Get all active forms
///         var forms = await _formRepository.GetActiveFormsAsync();
///         return forms;
///     }
///     
///     public async Task&lt;Form?&gt; GetFormForDisplayAsync(Guid formId)
///     {
///         // Get form with all questions and options
///         return await _formRepository.GetFormWithQuestionsAsync(formId);
///     }
/// }
/// </code>
/// </example>
public interface IFormRepository : IRepository<Form>
{
    /// <summary>
    /// Retrieves all active forms.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a list of forms where <see cref="Form.IsActive"/> is true.
    /// </returns>
    /// <remarks>
    /// Active forms are those currently accepting responses. Use this method to display
    /// available forms to users. Results are ordered by Name for consistency.
    /// </remarks>
    /// <example>
    /// <code>
    /// var activeForms = await _formRepository.GetActiveFormsAsync();
    /// foreach (var form in activeForms)
    /// {
    ///     Console.WriteLine($"{form.Name}: {form.Description}");
    /// }
    /// </code>
    /// </example>
    Task<IList<Form>> GetActiveFormsAsync();

    /// <summary>
    /// Retrieves a form with all related questions and their options.
    /// </summary>
    /// <param name="formId">The unique identifier of the form.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the form with questions and options loaded,
    /// or null if the form is not found.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method eager-loads all related data needed to display the form:
    /// - All questions (ordered by DisplayOrder)
    /// - All options for each question (ordered by DisplayOrder)
    /// </para>
    /// <para>
    /// Use this when rendering a form for users to fill out, as it provides
    /// all necessary data in a single database query.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var form = await _formRepository.GetFormWithQuestionsAsync(formId);
    /// if (form != null)
    /// {
    ///     foreach (var question in form.Questions.OrderBy(q => q.DisplayOrder))
    ///     {
    ///         Console.WriteLine(question.QuestionText);
    ///         foreach (var option in question.Options.OrderBy(o => o.DisplayOrder))
    ///         {
    ///             Console.WriteLine($"  - {option.OptionText}");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    Task<Form?> GetFormWithQuestionsAsync(Guid formId);

    /// <summary>
    /// Gets the total count of responses submitted for a specific form.
    /// </summary>
    /// <param name="formId">The unique identifier of the form.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the count of all responses (complete and incomplete).
    /// </returns>
    /// <remarks>
    /// This includes both complete and incomplete responses. To get only complete
    /// responses, you may need to filter further or create a separate method.
    /// </remarks>
    /// <example>
    /// <code>
    /// var responseCount = await _formRepository.GetFormResponseCountAsync(formId);
    /// Console.WriteLine($"Form has {responseCount} total responses");
    /// </code>
    /// </example>
    Task<int> GetFormResponseCountAsync(Guid formId);

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
    /// Only counts responses marked as complete. Useful for analytics and
    /// determining how many users successfully submitted the form.
    /// </remarks>
    /// <example>
    /// <code>
    /// var completeCount = await _formRepository.GetCompleteResponseCountAsync(formId);
    /// var totalCount = await _formRepository.GetFormResponseCountAsync(formId);
    /// var completionRate = (double)completeCount / totalCount * 100;
    /// Console.WriteLine($"Completion rate: {completionRate:F1}%");
    /// </code>
    /// </example>
    Task<int> GetCompleteResponseCountAsync(Guid formId);

    /// <summary>
    /// Gets a question type by its code.
    /// </summary>
    /// <param name="code">The question type code (e.g., "ShortText", "MultipleChoice").</param>
    /// <returns>
    /// The <see cref="QuestionType"/> with the specified code, or null if not found.
    /// </returns>
    /// <remarks>
    /// Use this to resolve question type codes from DTOs to entity IDs when creating/updating questions.
    /// </remarks>
    Task<QuestionType?> GetQuestionTypeByCodeAsync(string code);

    /// <summary>
    /// Gets all available question types.
    /// </summary>
    /// <returns>
    /// A list of all <see cref="QuestionType"/> entities, ordered by DisplayOrder.
    /// </returns>
    /// <remarks>
    /// Use this to populate question type dropdowns in form builders.
    /// </remarks>
    Task<IList<QuestionType>> GetAllQuestionTypesAsync();

    /// <summary>
    /// Gets a question type by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the question type.</param>
    /// <returns>
    /// The <see cref="QuestionType"/> with the specified ID, or null if not found.
    /// </returns>
    Task<QuestionType?> GetQuestionTypeByIdAsync(Guid id);

    /// <summary>
    /// Creates a new question type.
    /// </summary>
    /// <param name="questionType">The question type to create.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the created <see cref="QuestionType"/> with assigned ID.
    /// </returns>
    Task<QuestionType> CreateQuestionTypeAsync(QuestionType questionType);

    /// <summary>
    /// Updates an existing question type.
    /// </summary>
    /// <param name="questionType">The question type to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateQuestionTypeAsync(QuestionType questionType);

    /// <summary>
    /// Deletes a question type.
    /// </summary>
    /// <param name="id">The unique identifier of the question type to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This will fail if there are questions referencing this question type.
    /// </remarks>
    Task DeleteQuestionTypeAsync(Guid id);

    /// <summary>
    /// Saves a form response with all question responses.
    /// </summary>
    /// <param name="formResponse">The form response to save.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the saved <see cref="FormResponse"/> with assigned IDs.
    /// </returns>
    /// <remarks>
    /// This method saves the FormResponse and all associated QuestionResponses and
    /// QuestionResponseOptions in a single transaction. All entities will have their
    /// audit fields (CreatedOn, CreatedById) populated automatically.
    /// </remarks>
    Task<FormResponse> SaveFormResponseAsync(FormResponse formResponse);

    /// <summary>
    /// Gets a form response by ID with all question responses.
    /// </summary>
    /// <param name="responseId">The unique identifier of the response.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the response with all question responses and selected options loaded,
    /// or null if not found.
    /// </returns>
    Task<FormResponse?> GetFormResponseAsync(Guid responseId);

    /// <summary>
    /// Deletes all responses for a specific form.
    /// </summary>
    /// <param name="formId">The unique identifier of the form.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the number of responses deleted.
    /// </returns>
    /// <remarks>
    /// This performs a hard delete, permanently removing all responses for the form.
    /// Use with caution - this operation cannot be undone.
    /// </remarks>
    Task<int> DeleteAllFormResponsesAsync(Guid formId);

    /// <summary>
    /// Updates a form including its questions and options.
    /// </summary>
    /// <param name="formId">The ID of the form to update.</param>
    /// <param name="updateDto">The update data.</param>
    /// <returns>A task representing the operation.</returns>
    Task UpdateFormWithQuestionsAsync(Guid formId, UpdateFormDto updateDto);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// Use this method when you've modified tracked entities and need to persist changes
    /// without calling UpdateAsync (which assumes entities are detached).
    /// </remarks>
    Task SaveChangesAsync();
}
