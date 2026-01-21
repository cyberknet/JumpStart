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
using AutoMapper;
using JumpStart.Api.DTOs.Forms;
using JumpStart.Data.Advanced.Auditing;
using JumpStart.Forms;
using JumpStart.Repositories.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JumpStart.Api.Controllers.Forms;

/// <summary>
/// API controller for managing forms and form responses.
/// </summary>
/// <remarks>
/// <para>
/// This controller provides RESTful endpoints for creating and managing forms,
/// retrieving form definitions with questions, and handling form responses.
/// All endpoints follow standard HTTP conventions and return appropriate status codes.
/// </para>
/// <para>
/// <strong>Key Operations:</strong>
/// - List all forms or filter by active status
/// - Retrieve form details including questions and options
/// - Create, update, and delete forms
/// - Submit and retrieve form responses
/// - Get form statistics (response counts)
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Register controller in API project
/// builder.Services.AddJumpStart(options =>
/// {
///     options.RegisterFormsController = true;
///     options.RegisterUserContext&lt;ApiUserContext&gt;();
/// });
/// 
/// builder.Services.AddControllers();
/// 
/// // Endpoints are available at:
/// // GET    /api/forms
/// // GET    /api/forms/{id}
/// // POST   /api/forms
/// // PUT    /api/forms/{id}
/// // DELETE /api/forms/{id}
/// // GET    /api/forms/active
/// // GET    /api/forms/{id}/statistics
/// </code>
/// </example>
[ApiController]
[Route("api/[controller]")]
public class FormsController(
    IFormRepository formRepository,
    IMapper mapper,
    ILogger<FormsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets all forms.
    /// </summary>
    /// <returns>
    /// A list of all forms (active and inactive).
    /// Returns 200 OK with the list of forms.
    /// </returns>
    /// <remarks>
    /// This endpoint returns all forms regardless of their active status.
    /// For only active forms, use the /active endpoint instead.
    /// </remarks>
    /// <response code="200">Returns the list of all forms.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FormDto>), 200)]
    public async Task<ActionResult<IEnumerable<FormDto>>> GetAllForms()
    {
        logger.LogInformation("Retrieving all forms");
        
        var forms = await formRepository.GetAllAsync();
        var formDtos = mapper.Map<IEnumerable<FormDto>>(forms);
        
        return Ok(formDtos);
    }

    /// <summary>
    /// Gets all active forms.
    /// </summary>
    /// <returns>
    /// A list of forms where IsActive is true.
    /// Returns 200 OK with the list of active forms.
    /// </returns>
    /// <remarks>
    /// This endpoint is useful for displaying available forms to users.
    /// Inactive forms are excluded from the results.
    /// </remarks>
    /// <response code="200">Returns the list of active forms.</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<FormDto>), 200)]
    public async Task<ActionResult<IEnumerable<FormDto>>> GetActiveForms()
    {
        logger.LogInformation("Retrieving active forms");
        
        var forms = await formRepository.GetActiveFormsAsync();
        var formDtos = mapper.Map<IEnumerable<FormDto>>(forms);
        
        return Ok(formDtos);
    }

    /// <summary>
    /// Gets a specific form by ID, including all questions and options.
    /// </summary>
    /// <param name="id">The unique identifier of the form.</param>
    /// <returns>
    /// The complete form definition with all questions and their options.
    /// Returns 200 OK if found, 404 Not Found if the form doesn't exist.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This endpoint returns the complete form structure needed to display
    /// the form to users, including:
    /// - Form metadata (name, description, settings)
    /// - All questions ordered by DisplayOrder
    /// - All options for each question ordered by DisplayOrder
    /// </para>
    /// <para>
    /// Use this endpoint when rendering a form for users to fill out.
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the form with all questions and options.</response>
    /// <response code="404">The form was not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FormWithQuestionsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FormWithQuestionsDto>> GetFormById(Guid id)
    {
        logger.LogInformation("Retrieving form {FormId}", id);
        
        var form = await formRepository.GetFormWithQuestionsAsync(id);
        
        if (form == null)
        {
            logger.LogWarning("Form {FormId} not found", id);
            return NotFound(new { message = $"Form with ID {id} not found." });
        }
        
        var formDto = mapper.Map<FormWithQuestionsDto>(form);
        return Ok(formDto);
    }

    /// <summary>
    /// Creates a new form.
    /// </summary>
    /// <param name="createDto">The form creation data.</param>
    /// <returns>
    /// The created form with its assigned ID.
    /// Returns 201 Created with a Location header pointing to the new form.
    /// Returns 400 Bad Request if validation fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Creates a new form with the specified questions and options.
    /// The form is created in an inactive state by default - set IsActive to true
    /// in the request to immediately make it available to users.
    /// </para>
    /// <para>
    /// Questions and options will be assigned DisplayOrder values if not specified,
    /// based on their order in the collections.
    /// </para>
    /// </remarks>
    /// <response code="201">The form was created successfully.</response>
    /// <response code="400">The request was invalid (validation failure).</response>
    [HttpPost]
    [ProducesResponseType(typeof(FormDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<FormDto>> CreateForm([FromBody] CreateFormDto createDto)
    {
        logger.LogInformation("Creating new form: {FormName}", createDto.Name);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var form = mapper.Map<Form>(createDto);

        // Validate and set DisplayOrder for questions
        var validationErrors = new Dictionary<int, List<string>>();
        for (int i = 0; i < form.Questions.Count; i++)
        {
            var question = form.Questions.ElementAt(i);

            // Ensure DisplayOrder is set
            if (question.DisplayOrder == 0)
            {
                question.DisplayOrder = i + 1;
            }

            // Validate question definition (min/max constraints)
            var errors = ValidateQuestionDefinition(question);
            if (errors.Any())
            {
                validationErrors[i + 1] = errors;
            }

            // Ensure DisplayOrder is set for options
            for (int j = 0; j < question.Options.Count; j++)
            {
                var option = question.Options.ElementAt(j);
                if (option.DisplayOrder == 0)
                {
                    option.DisplayOrder = j + 1;
                }
            }
        }

        // Return validation errors if any
        if (validationErrors.Any())
        {
            logger.LogWarning("Form validation failed for {FormName}: {ErrorCount} question(s) have errors", 
                createDto.Name, validationErrors.Count);

            return BadRequest(new 
            { 
                message = "One or more questions have validation errors",
                questionErrors = validationErrors
            });
        }

        var createdForm = await formRepository.AddAsync(form);
        var formDto = mapper.Map<FormDto>(createdForm);

        logger.LogInformation("Form {FormId} created successfully", createdForm.Id);

        return CreatedAtAction(
            nameof(GetFormById),
            new { id = createdForm.Id },
            formDto);
    }

    /// <summary>
    /// Validates a question's min/max constraints are valid.
    /// </summary>
    private List<string> ValidateQuestionDefinition(Question question)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(question.MinimumValue) && string.IsNullOrEmpty(question.MaximumValue))
        {
            return errors; // No constraints to validate
        }

        switch (question.QuestionType.Code)
        {
            case "Number":
                ValidateNumericConstraints(question, errors);
                break;
            case "ShortText":
            case "LongText":
                ValidateTextConstraints(question, errors);
                break;
            case "Date":
                ValidateDateConstraints(question, errors);
                break;
        }

        return errors;
    }

    private void ValidateNumericConstraints(Question question, List<string> errors)
    {
        decimal? min = null, max = null;

        if (!string.IsNullOrEmpty(question.MinimumValue))
        {
            if (!decimal.TryParse(question.MinimumValue, out var minVal))
            {
                errors.Add($"MinimumValue '{question.MinimumValue}' is not a valid number");
            }
            else
            {
                min = minVal;
            }
        }

        if (!string.IsNullOrEmpty(question.MaximumValue))
        {
            if (!decimal.TryParse(question.MaximumValue, out var maxVal))
            {
                errors.Add($"MaximumValue '{question.MaximumValue}' is not a valid number");
            }
            else
            {
                max = maxVal;
            }
        }

        if (min.HasValue && max.HasValue && min.Value > max.Value)
        {
            errors.Add($"MinimumValue ({min}) cannot be greater than MaximumValue ({max})");
        }
    }

    private void ValidateTextConstraints(Question question, List<string> errors)
    {
        int? min = null, max = null;

        if (!string.IsNullOrEmpty(question.MinimumValue))
        {
            if (!int.TryParse(question.MinimumValue, out var minVal) || minVal < 0)
            {
                errors.Add($"MinimumValue '{question.MinimumValue}' must be a non-negative integer");
            }
            else
            {
                min = minVal;
            }
        }

        if (!string.IsNullOrEmpty(question.MaximumValue))
        {
            if (!int.TryParse(question.MaximumValue, out var maxVal) || maxVal < 0)
            {
                errors.Add($"MaximumValue '{question.MaximumValue}' must be a non-negative integer");
            }
            else
            {
                max = maxVal;
            }
        }

        if (min.HasValue && max.HasValue && min.Value > max.Value)
        {
            errors.Add($"MinimumValue ({min}) cannot be greater than MaximumValue ({max})");
        }
    }

    private void ValidateDateConstraints(Question question, List<string> errors)
    {
        DateTime? min = null, max = null;

        if (!string.IsNullOrEmpty(question.MinimumValue))
        {
            if (!DateTime.TryParse(question.MinimumValue, out var minVal))
            {
                errors.Add($"MinimumValue '{question.MinimumValue}' is not a valid date");
            }
            else
            {
                min = minVal;
            }
        }

        if (!string.IsNullOrEmpty(question.MaximumValue))
        {
            if (!DateTime.TryParse(question.MaximumValue, out var maxVal))
            {
                errors.Add($"MaximumValue '{question.MaximumValue}' is not a valid date");
            }
            else
            {
                max = maxVal;
            }
        }

        if (min.HasValue && max.HasValue && min.Value > max.Value)
        {
            errors.Add($"MinimumValue ({min:d}) cannot be greater than MaximumValue ({max:d})");
        }
    }

    /// <summary>
    /// Updates an existing form.
    /// </summary>
    /// <param name="id">The unique identifier of the form to update.</param>
    /// <param name="updateDto">The updated form data.</param>
    /// <returns>
    /// Returns 204 No Content if successful.
    /// Returns 404 Not Found if the form doesn't exist.
    /// Returns 400 Bad Request if validation fails or ID mismatch.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Updates the specified form. Note that this is a full update operation -
    /// all properties should be provided, even if not changed.
    /// </para>
    /// <para>
    /// <strong>Important:</strong>
    /// Updating a form that has existing responses may affect data integrity.
    /// Consider creating a new version of the form instead if responses exist.
    /// </para>
    /// </remarks>
    /// <response code="204">The form was updated successfully.</response>
    /// <response code="400">The request was invalid (validation failure or ID mismatch).</response>
    /// <response code="404">The form was not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateForm(Guid id, [FromBody] UpdateFormDto updateDto)
    {
        logger.LogInformation("Updating form {FormId}", id);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (id != updateDto.Id)
        {
            logger.LogWarning("Form ID mismatch: URL={UrlId}, Body={BodyId}", id, updateDto.Id);
            return BadRequest(new { message = "Form ID in URL does not match ID in request body." });
        }
        
        try
        {
            await formRepository.UpdateFormWithQuestionsAsync(id, updateDto);
            logger.LogInformation("Form {FormId} updated successfully", id);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            logger.LogWarning("Form {FormId} not found for update", id);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a form (soft delete).
    /// </summary>
    /// <param name="id">The unique identifier of the form to delete.</param>
    /// <returns>
    /// Returns 204 No Content if successful.
    /// Returns 404 Not Found if the form doesn't exist.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Performs a soft delete - the form is marked as deleted but not removed from the database.
    /// All associated questions, options, and responses are preserved.
    /// </para>
    /// <para>
    /// Deleted forms will not appear in GetAll or GetActive results but can still
    /// be retrieved by ID for audit purposes.
    /// </para>
    /// </remarks>
    /// <response code="204">The form was deleted successfully.</response>
    /// <response code="404">The form was not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteForm(Guid id)
    {
        logger.LogInformation("Deleting form {FormId}", id);
        
        var form = await formRepository.GetByIdAsync(id);
        if (form == null)
        {
            logger.LogWarning("Form {FormId} not found for deletion", id);
            return NotFound(new { message = $"Form with ID {id} not found." });
        }
        
        await formRepository.DeleteAsync(id);
        
        logger.LogInformation("Form {FormId} deleted successfully", id);
        
        return NoContent();
    }

        /// <summary>
        /// Gets statistics for a specific form.
        /// </summary>
        /// <param name="id">The unique identifier of the form.</param>
        /// <returns>
        /// Statistics including total responses and completion count.
        /// Returns 200 OK with statistics, 404 Not Found if form doesn't exist.
        /// </returns>
        /// <remarks>
        /// Provides aggregate statistics about form responses, useful for
        /// analytics and reporting.
        /// </remarks>
        /// <response code="200">Returns the form statistics.</response>
        /// <response code="404">The form was not found.</response>
        [HttpGet("{id:guid}/statistics")]
        [ProducesResponseType(typeof(FormStatisticsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FormStatisticsDto>> GetFormStatistics(Guid id)
        {
            logger.LogInformation("Retrieving statistics for form {FormId}", id);

            var form = await formRepository.GetByIdAsync(id);
            if (form == null)
            {
                logger.LogWarning("Form {FormId} not found", id);
                return NotFound(new { message = $"Form with ID {id} not found." });
            }

            var totalResponses = await formRepository.GetFormResponseCountAsync(id);
            var completeResponses = await formRepository.GetCompleteResponseCountAsync(id);

            var statistics = new FormStatisticsDto
            {
                FormId = id,
                FormName = form.Name,
                TotalResponses = totalResponses,
                CompleteResponses = completeResponses,
                IncompleteResponses = totalResponses - completeResponses,
                CompletionRate = totalResponses > 0 
                    ? (double)completeResponses / totalResponses * 100 
                    : 0
            };

            return Ok(statistics);
        }

            /// <summary>
            /// Gets all available question types.
            /// </summary>
            /// <returns>
            /// A list of all question types ordered by display order.
            /// Returns 200 OK with the list of question types.
            /// </returns>
            /// <remarks>
            /// Use this endpoint to populate question type dropdowns in form builders.
            /// Question types define how questions are rendered and whether they require options.
            /// </remarks>
            /// <response code="200">Returns the list of question types.</response>
            [HttpGet("question-types")]
            [ProducesResponseType(typeof(IEnumerable<QuestionTypeDto>), 200)]
            public async Task<ActionResult<IEnumerable<QuestionTypeDto>>> GetQuestionTypes()
            {
                logger.LogInformation("Retrieving all question types");

                var questionTypes = await formRepository.GetAllQuestionTypesAsync();
                var dtos = mapper.Map<IEnumerable<QuestionTypeDto>>(questionTypes);

                return Ok(dtos);
            }

            /// <summary>
            /// Submits a response to a form.
            /// </summary>
            /// <param name="formId">The unique identifier of the form.</param>
            /// <param name="createDto">The form response data.</param>
            /// <returns>
            /// The saved response with assigned ID.
            /// Returns 201 Created with the response details.
            /// Returns 400 Bad Request if validation fails.
            /// Returns 404 Not Found if the form doesn't exist.
            /// </returns>
            /// <remarks>
            /// <para>
            /// Validates the form exists and is active before accepting the response.
            /// Validates each question response against the question's constraints.
            /// </para>
            /// <para>
            /// For anonymous responses, set RespondentUserId to null.
            /// Set IsComplete to false for draft/partial responses.
            /// </para>
            /// </remarks>
            /// <response code="201">The response was submitted successfully.</response>
            /// <response code="400">The request was invalid (validation failure).</response>
            /// <response code="404">The form was not found or is inactive.</response>
            [HttpPost("{formId:guid}/responses")]
            [ProducesResponseType(typeof(FormResponseDto), 201)]
            [ProducesResponseType(400)]
            [ProducesResponseType(404)]
            public async Task<ActionResult<FormResponseDto>> SubmitFormResponse(
                Guid formId, 
                [FromBody] CreateFormResponseDto createDto)
            {
                logger.LogInformation("Submitting response for form {FormId}", formId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verify form exists and is active
                var form = await formRepository.GetFormWithQuestionsAsync(formId);
                if (form == null)
                {
                    logger.LogWarning("Form {FormId} not found", formId);
                    return NotFound(new { message = $"Form with ID {formId} not found." });
                }

                if (!form.IsActive)
                {
                    logger.LogWarning("Attempted to submit response to inactive form {FormId}", formId);
                    return BadRequest(new { message = "This form is no longer accepting responses." });
                }

                // Validate question responses
                var validationErrors = new Dictionary<Guid, List<string>>();
                foreach (var questionResponseDto in createDto.QuestionResponses)
                {
                    var question = form.Questions.FirstOrDefault(q => q.Id == questionResponseDto.QuestionId);
                    if (question == null)
                    {
                        validationErrors[questionResponseDto.QuestionId] = new List<string> 
                        { 
                            $"Question {questionResponseDto.QuestionId} does not belong to this form" 
                        };
                        continue;
                    }

                    // Validate response value against question constraints
                    if (!string.IsNullOrEmpty(questionResponseDto.ResponseValue))
                    {
                        var isValid = QuestionValidator.ValidateResponseValue(
                            question, 
                            questionResponseDto.ResponseValue);

                        if (!isValid)
                        {
                            var errors = new List<string>();

                            if (question.IsRequired && string.IsNullOrWhiteSpace(questionResponseDto.ResponseValue))
                            {
                                errors.Add("This question is required");
                            }
                            else
                            {
                                errors.Add("Response does not meet question constraints");
                            }

                            validationErrors[questionResponseDto.QuestionId] = errors;
                        }
                    }
                    else if (question.IsRequired && !questionResponseDto.SelectedOptionIds.Any())
                    {
                        validationErrors[questionResponseDto.QuestionId] = new List<string> 
                        { 
                            "This question is required" 
                        };
                    }
                }

                if (validationErrors.Any())
                {
                    logger.LogWarning("Form response validation failed for form {FormId}", formId);
                    return BadRequest(new 
                    { 
                        message = "One or more responses are invalid",
                        errors = validationErrors
                    });
                }

                // Map DTO to entity
                var formResponse = mapper.Map<FormResponse>(createDto);
                formResponse.FormId = formId;
                formResponse.SubmittedOn = DateTime.UtcNow;

                // Save response
                var savedResponse = await formRepository.SaveFormResponseAsync(formResponse);

                // Load the saved response with all related data
                var responseWithData = await formRepository.GetFormResponseAsync(savedResponse.Id);
                var responseDto = mapper.Map<FormResponseDto>(responseWithData);

                logger.LogInformation("Form response {ResponseId} submitted successfully for form {FormId}", 
                    savedResponse.Id, formId);

                return CreatedAtAction(
                    nameof(GetFormResponseById),
                    new { formId, responseId = savedResponse.Id },
                    responseDto);
            }

                /// <summary>
                /// Gets a specific form response by ID.
                /// </summary>
                /// <param name="formId">The unique identifier of the form.</param>
                /// <param name="responseId">The unique identifier of the response.</param>
                /// <returns>
                /// The form response with all question responses.
                /// Returns 200 OK if found, 404 Not Found if not found.
                /// </returns>
                /// <response code="200">Returns the form response.</response>
                /// <response code="404">The response was not found.</response>
                [HttpGet("{formId:guid}/responses/{responseId:guid}")]
                [ProducesResponseType(typeof(FormResponseDto), 200)]
                [ProducesResponseType(404)]
                public async Task<ActionResult<FormResponseDto>> GetFormResponseById(Guid formId, Guid responseId)
                {
                    logger.LogInformation("Retrieving response {ResponseId} for form {FormId}", responseId, formId);

                    var response = await formRepository.GetFormResponseAsync(responseId);

                    if (response == null || response.FormId != formId)
                    {
                        logger.LogWarning("Response {ResponseId} not found for form {FormId}", responseId, formId);
                        return NotFound(new { message = $"Response with ID {responseId} not found for this form." });
                    }

                    var responseDto = mapper.Map<FormResponseDto>(response);
                    return Ok(responseDto);
                }

                /// <summary>
                /// Deletes all responses for a specific form.
                /// </summary>
                /// <param name="formId">The unique identifier of the form.</param>
                /// <returns>
                /// Returns 200 OK with the count of deleted responses.
                /// Returns 404 Not Found if the form doesn't exist.
                /// </returns>
                /// <remarks>
                /// <para>
                /// <strong>WARNING:</strong> This permanently deletes all responses for the form.
                /// This operation cannot be undone.
                /// </para>
                /// <para>
                /// Use this endpoint when you need to clear all responses before editing a form,
                /// or when responses need to be purged for data privacy reasons.
                /// </para>
                /// </remarks>
                /// <response code="200">Returns the number of responses deleted.</response>
                /// <response code="404">The form was not found.</response>
                [HttpDelete("{formId:guid}/responses")]
                [ProducesResponseType(typeof(int), 200)]
                [ProducesResponseType(404)]
                public async Task<ActionResult<int>> DeleteAllFormResponses(Guid formId)
                {
                    logger.LogInformation("Deleting all responses for form {FormId}", formId);

                    // Verify form exists
                    var form = await formRepository.GetByIdAsync(formId);
                    if (form == null)
                    {
                        logger.LogWarning("Form {FormId} not found", formId);
                        return NotFound(new { message = $"Form with ID {formId} not found." });
                    }

                    var deletedCount = await formRepository.DeleteAllFormResponsesAsync(formId);

                    logger.LogInformation("Deleted {Count} responses for form {FormId}", deletedCount, formId);

                            return Ok(deletedCount);
                        }

                        /// <summary>
                        /// Creates a new question type.
                        /// </summary>
                        /// <param name="createDto">The question type creation data.</param>
                        /// <returns>
                        /// The created question type with its assigned ID.
                        /// Returns 201 Created with the question type details.
                        /// Returns 400 Bad Request if validation fails or a type with the same code already exists.
                        /// </returns>
                        /// <response code="201">The question type was created successfully.</response>
                        /// <response code="400">The request was invalid.</response>
                        [HttpPost("question-types")]
                        [ProducesResponseType(typeof(QuestionTypeDto), 201)]
                        [ProducesResponseType(400)]
                        public async Task<ActionResult<QuestionTypeDto>> CreateQuestionType([FromBody] CreateQuestionTypeDto createDto)
                        {
                            logger.LogInformation("Creating question type with code {Code}", createDto.Code);

                            if (!ModelState.IsValid)
                            {
                                return BadRequest(ModelState);
                            }

                            // Check if a question type with the same code already exists
                            var existing = await formRepository.GetQuestionTypeByCodeAsync(createDto.Code);
                            if (existing != null)
                            {
                                logger.LogWarning("Question type with code {Code} already exists", createDto.Code);
                                return BadRequest(new { message = $"A question type with code '{createDto.Code}' already exists." });
                            }

                            var questionType = mapper.Map<QuestionType>(createDto);
                            var created = await formRepository.CreateQuestionTypeAsync(questionType);

                            var dto = mapper.Map<QuestionTypeDto>(created);
                            logger.LogInformation("Created question type {Id} with code {Code}", created.Id, created.Code);

                            return CreatedAtAction(nameof(GetQuestionTypeById), new { id = created.Id }, dto);
                        }

                        /// <summary>
                        /// Gets a specific question type by ID.
                        /// </summary>
                        /// <param name="id">The unique identifier of the question type.</param>
                        /// <returns>
                        /// The question type details.
                        /// Returns 200 OK if found, 404 Not Found if the question type doesn't exist.
                        /// </returns>
                        /// <response code="200">Returns the question type.</response>
                        /// <response code="404">The question type was not found.</response>
                        [HttpGet("question-types/{id:guid}")]
                        [ProducesResponseType(typeof(QuestionTypeDto), 200)]
                        [ProducesResponseType(404)]
                        public async Task<ActionResult<QuestionTypeDto>> GetQuestionTypeById(Guid id)
                        {
                            logger.LogInformation("Retrieving question type {Id}", id);

                            var questionType = await formRepository.GetQuestionTypeByIdAsync(id);
                            if (questionType == null)
                            {
                                logger.LogWarning("Question type {Id} not found", id);
                                return NotFound(new { message = $"Question type with ID {id} not found." });
                            }

                            var dto = mapper.Map<QuestionTypeDto>(questionType);
                            return Ok(dto);
                        }

                        /// <summary>
                        /// Updates an existing question type.
                        /// </summary>
                        /// <param name="id">The unique identifier of the question type to update.</param>
                        /// <param name="updateDto">The updated question type data.</param>
                        /// <returns>
                        /// Returns 204 No Content if successful.
                        /// Returns 400 Bad Request if validation fails.
                        /// Returns 404 Not Found if the question type doesn't exist.
                        /// </returns>
                        /// <response code="204">The question type was updated successfully.</response>
                        /// <response code="400">The request was invalid.</response>
                        /// <response code="404">The question type was not found.</response>
                        [HttpPut("question-types/{id:guid}")]
                        [ProducesResponseType(204)]
                        [ProducesResponseType(400)]
                        [ProducesResponseType(404)]
                        public async Task<IActionResult> UpdateQuestionType(Guid id, [FromBody] UpdateQuestionTypeDto updateDto)
                        {
                            logger.LogInformation("Updating question type {Id}", id);

                            if (!ModelState.IsValid)
                            {
                                return BadRequest(ModelState);
                            }

                            var questionType = await formRepository.GetQuestionTypeByIdAsync(id);
                            if (questionType == null)
                            {
                                logger.LogWarning("Question type {Id} not found", id);
                                return NotFound(new { message = $"Question type with ID {id} not found." });
                            }

                            // Update only provided fields
                            if (updateDto.Code != null) questionType.Code = updateDto.Code;
                            if (updateDto.Name != null) questionType.Name = updateDto.Name;
                            if (updateDto.Description != null) questionType.Description = updateDto.Description;
                            if (updateDto.HasOptions.HasValue) questionType.HasOptions = updateDto.HasOptions.Value;
                            if (updateDto.AllowsMultipleValues.HasValue) questionType.AllowsMultipleValues = updateDto.AllowsMultipleValues.Value;
                            if (updateDto.InputType != null) questionType.InputType = updateDto.InputType;
                            if (updateDto.DisplayOrder.HasValue) questionType.DisplayOrder = updateDto.DisplayOrder.Value;
                            if (updateDto.ApplicationData != null) questionType.ApplicationData = updateDto.ApplicationData;

                            await formRepository.UpdateQuestionTypeAsync(questionType);

                            logger.LogInformation("Updated question type {Id}", id);
                            return NoContent();
                        }

                        /// <summary>
                        /// Deletes a question type.
                        /// </summary>
                        /// <param name="id">The unique identifier of the question type to delete.</param>
                        /// <returns>
                        /// Returns 204 No Content if successful.
                        /// Returns 404 Not Found if the question type doesn't exist.
                        /// Returns 400 Bad Request if the question type is in use.
                        /// </returns>
                        /// <remarks>
                        /// This will fail if there are questions referencing this question type.
                        /// </remarks>
                        /// <response code="204">The question type was deleted successfully.</response>
                        /// <response code="400">The question type is in use and cannot be deleted.</response>
                        /// <response code="404">The question type was not found.</response>
                        [HttpDelete("question-types/{id:guid}")]
                        [ProducesResponseType(204)]
                        [ProducesResponseType(400)]
                        [ProducesResponseType(404)]
                        public async Task<IActionResult> DeleteQuestionType(Guid id)
                        {
                            logger.LogInformation("Deleting question type {Id}", id);

                            var questionType = await formRepository.GetQuestionTypeByIdAsync(id);
                            if (questionType == null)
                            {
                                logger.LogWarning("Question type {Id} not found", id);
                                return NotFound(new { message = $"Question type with ID {id} not found." });
                            }

                            try
                            {
                                await formRepository.DeleteQuestionTypeAsync(id);
                                logger.LogInformation("Deleted question type {Id}", id);
                                return NoContent();
                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning(ex, "Failed to delete question type {Id} - may be in use", id);
                                return BadRequest(new { message = "Cannot delete question type because it is in use by existing questions." });
                            }
                        }
                    }
