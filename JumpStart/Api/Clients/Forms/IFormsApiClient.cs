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
using JumpStart.Api.DTOs.Forms;
using Refit;

namespace JumpStart.Api.Clients.Forms;

/// <summary>
/// Refit-based API client for consuming Forms REST API endpoints.
/// </summary>
/// <remarks>
/// <para>
/// This client provides a strongly-typed interface for calling Forms API endpoints
/// from Blazor Server or other client applications. It uses Refit to generate
/// HTTP client implementations automatically.
/// </para>
/// <para>
/// <strong>Usage Scenarios:</strong>
/// - Blazor Server apps calling a separate Forms API
/// - Microservices architecture where Forms is a separate service
/// - Client applications consuming Forms functionality remotely
/// </para>
/// <para>
/// <strong>Registration:</strong>
/// Enable via JumpStart options:
/// <code>
/// builder.Services.AddJumpStart(options =>
/// {
///     options.ApiBaseUrl = "https://localhost:7030";
///     options.RegisterFormsApiClient = true;
/// });
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Inject and use in a Blazor component or service
/// @inject IFormsApiClient FormsClient
/// 
/// private async Task LoadFormsAsync()
/// {
///     var forms = await FormsClient.GetActiveFormsAsync();
///     foreach (var form in forms)
///     {
///         Console.WriteLine($"{form.Name}: {form.Description}");
///     }
/// }
/// 
/// // Create a new form
/// var newForm = new CreateFormDto
/// {
///     Name = "Customer Feedback",
///     Description = "Please rate our service",
///     IsActive = true
/// };
/// var created = await FormsClient.CreateFormAsync(newForm);
/// </code>
/// </example>
public interface IFormsApiClient
{
    /// <summary>
    /// Gets all forms (active and inactive).
    /// </summary>
    /// <returns>A list of all forms.</returns>
    [Get("/api/forms")]
    Task<IEnumerable<FormDto>> GetAllFormsAsync();

    /// <summary>
    /// Gets all active forms.
    /// </summary>
    /// <returns>A list of active forms where IsActive is true.</returns>
    [Get("/api/forms/active")]
    Task<IEnumerable<FormDto>> GetActiveFormsAsync();

    /// <summary>
    /// Gets a specific form by ID, including all questions and options.
    /// </summary>
    /// <param name="id">The unique identifier of the form.</param>
    /// <returns>The form with all questions and options, or null if not found.</returns>
    [Get("/api/forms/{id}")]
    Task<FormWithQuestionsDto> GetFormByIdAsync(Guid id);

    /// <summary>
    /// Creates a new form with questions and options.
    /// </summary>
    /// <param name="createDto">The form creation data.</param>
    /// <returns>The created form with its assigned ID.</returns>
    [Post("/api/forms")]
    Task<FormDto> CreateFormAsync([Body] CreateFormDto createDto);

    /// <summary>
    /// Updates an existing form.
    /// </summary>
    /// <param name="id">The unique identifier of the form to update.</param>
    /// <param name="updateDto">The updated form data.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Put("/api/forms/{id}")]
    Task UpdateFormAsync(Guid id, [Body] UpdateFormDto updateDto);

    /// <summary>
    /// Deletes a form (soft delete).
    /// </summary>
    /// <param name="id">The unique identifier of the form to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Delete("/api/forms/{id}")]
    Task DeleteFormAsync(Guid id);

        /// <summary>
        /// Gets statistics for a specific form.
        /// </summary>
        /// <param name="id">The unique identifier of the form.</param>
        /// <returns>Statistics including total responses and completion rate.</returns>
        [Get("/api/forms/{id}/statistics")]
        Task<FormStatisticsDto> GetFormStatisticsAsync(Guid id);

            /// <summary>
            /// Gets all available question types.
            /// </summary>
            /// <returns>A list of all question types ordered by display order.</returns>
            [Get("/api/forms/question-types")]
            Task<IEnumerable<QuestionTypeDto>> GetQuestionTypesAsync();

            /// <summary>
            /// Gets a specific question type by ID.
            /// </summary>
            /// <param name="id">The unique identifier of the question type.</param>
            /// <returns>The question type details.</returns>
            [Get("/api/forms/question-types/{id}")]
            Task<QuestionTypeDto> GetQuestionTypeByIdAsync(Guid id);

            /// <summary>
            /// Creates a new question type.
            /// </summary>
            /// <param name="createDto">The question type creation data.</param>
            /// <returns>The created question type with its assigned ID.</returns>
            [Post("/api/forms/question-types")]
            Task<QuestionTypeDto> CreateQuestionTypeAsync([Body] CreateQuestionTypeDto createDto);

            /// <summary>
            /// Updates an existing question type.
            /// </summary>
            /// <param name="id">The unique identifier of the question type to update.</param>
            /// <param name="updateDto">The updated question type data.</param>
            /// <returns>A task representing the asynchronous operation.</returns>
            [Put("/api/forms/question-types/{id}")]
            Task UpdateQuestionTypeAsync(Guid id, [Body] UpdateQuestionTypeDto updateDto);

            /// <summary>
            /// Deletes a question type.
            /// </summary>
            /// <param name="id">The unique identifier of the question type to delete.</param>
            /// <returns>A task representing the asynchronous operation.</returns>
            [Delete("/api/forms/question-types/{id}")]
            Task DeleteQuestionTypeAsync(Guid id);

            /// <summary>
            /// Submits a response to a form.
            /// </summary>
            /// <param name="formId">The unique identifier of the form.</param>
            /// <param name="createDto">The form response data.</param>
            /// <returns>The saved response with assigned ID.</returns>
            [Post("/api/forms/{formId}/responses")]
            Task<FormResponseDto> SubmitFormResponseAsync(Guid formId, [Body] CreateFormResponseDto createDto);

                /// <summary>
                /// Gets a specific form response by ID.
                /// </summary>
                /// <param name="formId">The unique identifier of the form.</param>
                /// <param name="responseId">The unique identifier of the response.</param>
                /// <returns>The form response with all question responses.</returns>
                [Get("/api/forms/{formId}/responses/{responseId}")]
                Task<FormResponseDto> GetFormResponseByIdAsync(Guid formId, Guid responseId);

                /// <summary>
                /// Deletes all responses for a specific form.
                /// </summary>
                /// <param name="formId">The unique identifier of the form.</param>
                /// <returns>The number of responses deleted.</returns>
                [Delete("/api/forms/{formId}/responses")]
                Task<int> DeleteAllFormResponsesAsync(Guid formId);
            }
