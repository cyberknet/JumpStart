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

using AutoMapper;
using Correlate;
using JumpStart.Api.Controllers;
using JumpStart.Data.Auditing;
using JumpStart.Forms;
using JumpStart.Forms.DTOs;
using JumpStart.Forms.Repositories;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JumpStart.Forms.Controllers;

/// <summary>
/// API controller for managing forms and form responses.
/// </summary>
/// <remarks>
/// <para>
/// Provides RESTful endpoints for creating, updating, retrieving, and deleting forms, as well as handling form responses and statistics. All endpoints follow standard HTTP conventions and return appropriate status codes.
/// </para>
/// <para>
/// <strong>Key Operations:</strong>
/// <list type="bullet">
/// <item>List all forms or filter by active status</item>
/// <item>Retrieve form details including questions and options</item>
/// <item>Create, update, and delete forms</item>
/// <item>Submit and retrieve form responses</item>
/// <item>Get form statistics (response counts)</item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code language="csharp">
/// // Register controller in API project
/// builder.Services.AddJumpStart(options =&gt;
/// {
///     options.RegisterFormsController = true;
///     options.RegisterUserContext&lt;JumpStart.Api.Controllers.ApiUserContext&gt;();
/// });
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
public class FormsController : ApiControllerBase<
        Form,
        FormDto,
        CreateFormDto,
        UpdateFormDto,
        IFormRepository>
{
    public FormsController(
        IFormRepository formRepository,
        AutoMapper.IMapper mapper,
        ILogger<FormsController> logger,
        ICorrelationContextAccessor correlationContext)
        : base(formRepository, mapper, logger, correlationContext)
    {
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
        _logger.LogInformation("Retrieving active forms");

        var forms = await _repository.GetActiveFormsAsync();
        var formDtos = _mapper.Map<IEnumerable<FormDto>>(forms);

        return Ok(formDtos);
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
    public override async Task<ActionResult<FormDto>> Update(Guid id, [FromBody] UpdateFormDto updateDto)
    {
        _logger.LogInformation("Updating form {FormId}", id);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != updateDto.Id)
        {
            _logger.LogWarning("Form ID mismatch: URL={UrlId}, Body={BodyId}", id, updateDto.Id);
            BadRequest(new { message = "Form ID in URL does not match ID in request body." });
        }

        try
        {
            await _repository.UpdateFormWithQuestionsAsync(id, updateDto);
            _logger.LogInformation("Form {FormId} updated successfully", id);

            // Fetch the updated form with questions
            var updatedForm = await _repository.GetFormWithQuestionsAsync(id);
            if (updatedForm == null)
            {
                _logger.LogWarning("Form {FormId} not found after update", id);
                return NotFound(new { message = $"Form with ID {id} not found after update." });
            }
            var formDto = _mapper.Map<FormDto>(updatedForm);
            return Ok(formDto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Form {FormId} not found for update", id);
            return NotFound(new { message = ex.Message });
        }
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
        _logger.LogInformation("Retrieving statistics for form {FormId}", id);

        var form = await _repository.GetByIdAsync(id, GetIncludesForGetById());
        if (form == null)
        {
            _logger.LogWarning("Form {FormId} not found", id);
            return NotFound(new { message = $"Form with ID {id} not found." });
        }

        var totalResponses = await _repository.GetFormResponseCountAsync(id);
        var completeResponses = await _repository.GetCompleteResponseCountAsync(id);

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
    /// Submits a response to a form.
    /// </summary>
    /// <param name="formId">The unique identifier of the form being responded to.</param>
    /// <param name="createDto">The form response data.</param>
    /// <returns>
    /// Returns 201 Created with the saved response.
    /// Returns 400 Bad Request if a question or option doesn't belong to this form, or if a
    /// response value fails validation.
    /// Returns 404 Not Found if the form doesn't exist.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Every submitted answer is validated against the form's actual questions rather than
    /// trusted as-is:
    /// </para>
    /// <list type="bullet">
    /// <item>Every <c>QuestionId</c> must belong to this form.</item>
    /// <item>Every selected option ID must belong to that specific question.</item>
    /// <item>Text/number/date answers are validated via <see cref="QuestionValidator.ValidateResponseValue"/>.</item>
    /// <item>Choice-based answers (including Ranking) are validated via
    /// <see cref="QuestionValidator.ValidateSelectedOptionCount"/>, which checks required/empty
    /// and selection-count constraints (e.g. Ranking's minimum/maximum item count).</item>
    /// </list>
    /// <para>
    /// <c>IsComplete</c> is computed server-side from whether every required question received
    /// an answer - the client-supplied value on <see cref="CreateFormResponseDto.IsComplete"/> is
    /// not trusted.
    /// </para>
    /// </remarks>
    /// <response code="201">The response was submitted successfully.</response>
    /// <response code="400">Validation failed - see the response body for details.</response>
    /// <response code="404">The form was not found.</response>
    [HttpPost("{formId:guid}/responses")]
    [ProducesResponseType(typeof(FormResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FormResponseDto>> SubmitFormResponse(Guid formId, [FromBody] CreateFormResponseDto createDto)
    {
        _logger.LogInformation("Submitting response for form {FormId}", formId);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var form = await _repository.GetFormWithQuestionsAsync(formId);
        if (form == null)
        {
            _logger.LogWarning("Form {FormId} not found", formId);
            return NotFound(new { message = $"Form with ID {formId} not found." });
        }

        var questionsById = form.Questions.ToDictionary(q => q.Id);
        var validationErrors = new List<string>();

        foreach (var answer in createDto.QuestionResponses)
        {
            if (!questionsById.TryGetValue(answer.QuestionId, out var question))
            {
                validationErrors.Add($"Question {answer.QuestionId} does not belong to form {formId}.");
                continue;
            }

            var selectedOptionIds = answer.SelectedOptionIds ?? [];
            var invalidOptionIds = selectedOptionIds
                .Where(optionId => !question.Options.Any(o => o.Id == optionId))
                .ToList();
            if (invalidOptionIds.Count > 0)
            {
                validationErrors.Add(
                    $"Question {answer.QuestionId}: option(s) {string.Join(", ", invalidOptionIds)} do not belong to this question.");
                continue;
            }

            // Choice-based questions answer via SelectedOptionIds (count-based constraints);
            // everything else answers via ResponseValue (value/length constraints).
            var isValidAnswer = question.QuestionType.HasOptions
                ? QuestionValidator.ValidateSelectedOptionCount(question, selectedOptionIds)
                : QuestionValidator.ValidateResponseValue(question, answer.ResponseValue);

            if (!isValidAnswer)
            {
                validationErrors.Add($"Question {answer.QuestionId} ('{question.QuestionText}'): response is invalid.");
            }
        }

        if (validationErrors.Any())
        {
            _logger.LogWarning("Form response validation failed for form {FormId}", formId);
            return BadRequest(new
            {
                message = "One or more responses are invalid",
                errors = validationErrors
            });
        }

        // Map DTO to entity
        var formResponse = _mapper.Map<FormResponse>(createDto);
        formResponse.FormId = formId;
        formResponse.SubmittedOn = DateTime.UtcNow;

        // IsComplete is computed server-side - the client's value is never trusted
        var answersByQuestionId = createDto.QuestionResponses.ToDictionary(a => a.QuestionId);
        formResponse.IsComplete = form.Questions
            .Where(q => q.IsRequired)
            .All(q => answersByQuestionId.TryGetValue(q.Id, out var answer) && HasAnswer(q, answer));

        // Save response
        var savedResponse = await _repository.SaveFormResponseAsync(formResponse);

        // Load the saved response with all related data
        var responseWithData = await _repository.GetFormResponseAsync(savedResponse.Id);
        var responseDto = _mapper.Map<FormResponseDto>(responseWithData);

        _logger.LogInformation("Form response {ResponseId} submitted successfully for form {FormId}",
                    savedResponse.Id, formId);

        return CreatedAtAction(
            nameof(GetFormResponseById),
            new { formId, responseId = savedResponse.Id },
            responseDto);
    }

    /// <summary>
    /// Determines whether a submitted answer satisfies a question's "has been answered" test -
    /// at least one selected option for choice-based questions, a non-empty value otherwise.
    /// </summary>
    private static bool HasAnswer(Question question, CreateQuestionResponseDto answer) =>
        question.QuestionType.HasOptions
            ? answer.SelectedOptionIds is { Count: > 0 }
            : !string.IsNullOrWhiteSpace(answer.ResponseValue);

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
        _logger.LogInformation("Retrieving response {ResponseId} for form {FormId}", responseId, formId);

        var response = await _repository.GetFormResponseAsync(responseId);

        if (response == null || response.FormId != formId)
        {
            _logger.LogWarning("Response {ResponseId} not found for form {FormId}", responseId, formId);
            return NotFound(new { message = $"Response with ID {responseId} not found for this form." });
        }

        var responseDto = _mapper.Map<FormResponseDto>(response);
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
        _logger.LogInformation("Deleting all responses for form {FormId}", formId);

        // Verify form exists
        var form = await _repository.GetByIdAsync(formId, GetIncludesForGetById());
        if (form == null)
        {
            _logger.LogWarning("Form {FormId} not found", formId);
            return NotFound(new { message = $"Form with ID {formId} not found." });
        }

        var deletedCount = await _repository.DeleteAllFormResponsesAsync(formId);

        _logger.LogInformation("Deleted {Count} responses for form {FormId}", deletedCount, formId);

        return Ok(deletedCount);
    }

    #region Overrides
    protected override Func<IQueryable<Form>, IQueryable<Form>>? GetIncludesForGetById() =>
        q => q
            .Include(f => f.Questions.OrderBy(q => q.DisplayOrder)) // include the Questions collection
                .ThenInclude(q => q.QuestionType)
            .Include(f => f.Questions)
                .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder));

    protected override (bool isValid, object? errorResult) OnBeforeCreate(Form form)
    {
        var validationErrors = new Dictionary<int, List<string>>();

        for (int i = 0; i < form.Questions.Count; i++)
        {
            var question = form.Questions.ElementAt(i);

            // Ensure DisplayOrder is set
            if (question.DisplayOrder == 0)
                question.DisplayOrder = i + 1;

            // Validate question definition (min/max constraints)
            var errors = ValidateQuestionDefinition(question);
            if (errors.Any())
                validationErrors[i + 1] = errors;

            // Ensure DisplayOrder is set for options
            for (int j = 0; j < question.Options.Count; j++)
            {
                var option = question.Options.ElementAt(j);
                if (option.DisplayOrder == 0)
                    option.DisplayOrder = j + 1;
            }
        }

        if (validationErrors.Any())
        {
            return (false, new
            {
                message = "One or more questions have validation errors",
                questionErrors = validationErrors
            });
        }

        return (true, null);
    }

    protected override (bool isValid, object? errorResult) OnBeforeUpdate(Form form)
    {

        return (true, null);
    }
    #endregion
}
