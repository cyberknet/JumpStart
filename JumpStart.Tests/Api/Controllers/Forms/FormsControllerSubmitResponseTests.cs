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
using JumpStart.Forms;
using JumpStart.Forms.Controllers;
using JumpStart.Forms.DTOs;
using JumpStart.Forms.Mapping;
using JumpStart.Forms.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace JumpStart.Tests.Api.Controllers.Forms;

/// <summary>
/// Tests for <see cref="FormsController.SubmitFormResponse"/>: referential integrity checks,
/// <see cref="QuestionValidator"/> wiring, and server-computed <c>IsComplete</c>.
/// </summary>
public class FormsControllerSubmitResponseTests
{
    private readonly Mock<IFormRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly FormsController _controller;

    private readonly Guid _formId = Guid.NewGuid();
    private readonly Guid _numberQuestionId = Guid.NewGuid();
    private readonly Guid _choiceQuestionId = Guid.NewGuid();
    private readonly Guid _optionAId = Guid.NewGuid();
    private readonly Guid _optionBId = Guid.NewGuid();

    public FormsControllerSubmitResponseTests()
    {
        _mockRepository = new Mock<IFormRepository>();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<FormProfile>();
            cfg.AddProfile<FormResponseProfile>();
            cfg.AddProfile<QuestionOptionProfile>();
            cfg.AddProfile<QuestionProfile>();
            cfg.AddProfile<QuestionResponseProfile>();
            cfg.AddProfile<QuestionTypeProfile>();
        }, loggerFactory);
        _mapper = config.CreateMapper();

        var mockLogger = new Mock<ILogger<FormsController>>();

        // CorrelationContext.CorrelationId is a plain, non-virtual property - not mockable via
        // Moq (which requires overridable members) - so construct a real instance instead.
        var correlationContext = new CorrelationContext { CorrelationId = "test-correlation-id" };
        var mockCorrelationContextAccessor = new Mock<ICorrelationContextAccessor>();
        mockCorrelationContextAccessor.SetupGet(a => a.CorrelationContext).Returns(correlationContext);

        _controller = new FormsController(_mockRepository.Object, _mapper, mockLogger.Object, mockCorrelationContextAccessor.Object);
    }

    private Form BuildForm()
    {
        var numberQuestionType = new QuestionType { Id = Guid.NewGuid(), Code = "Number", Name = "Number", HasOptions = false };
        var choiceQuestionType = new QuestionType { Id = Guid.NewGuid(), Code = "SingleChoice", Name = "Single Choice", HasOptions = true };

        var optionA = new QuestionOption { Id = _optionAId, QuestionId = _choiceQuestionId, OptionText = "A", DisplayOrder = 1 };
        var optionB = new QuestionOption { Id = _optionBId, QuestionId = _choiceQuestionId, OptionText = "B", DisplayOrder = 2 };

        var numberQuestion = new Question
        {
            Id = _numberQuestionId,
            FormId = _formId,
            QuestionText = "How many?",
            QuestionTypeId = numberQuestionType.Id,
            QuestionType = numberQuestionType,
            IsRequired = true,
            MinimumValue = "1",
            MaximumValue = "10"
        };

        var choiceQuestion = new Question
        {
            Id = _choiceQuestionId,
            FormId = _formId,
            QuestionText = "Pick one",
            QuestionTypeId = choiceQuestionType.Id,
            QuestionType = choiceQuestionType,
            IsRequired = true,
            Options = [optionA, optionB]
        };

        return new Form
        {
            Id = _formId,
            Name = "Test Form",
            Questions = [numberQuestion, choiceQuestion]
        };
    }

    /// <summary>
    /// Builds the fully-populated FormResponse the repository would return from
    /// GetFormResponseAsync after a successful save - AutoMapper's entity-to-DTO direction
    /// needs Form/Question/QuestionOption navigation properties populated to avoid null refs.
    /// </summary>
    private static FormResponse BuildReloadedResponse(FormResponse saved, Form form)
    {
        saved.Form = form;
        foreach (var answer in saved.Answers)
        {
            answer.Question = form.Questions.First(q => q.Id == answer.QuestionId);
            foreach (var selected in answer.SelectedOptions)
            {
                selected.QuestionOption = answer.Question.Options.First(o => o.Id == selected.QuestionOptionId);
            }
        }
        return saved;
    }

    [Fact]
    public async Task SubmitFormResponse_ReturnsNotFound_WhenFormDoesNotExist()
    {
        _mockRepository.Setup(r => r.GetFormWithQuestionsAsync(_formId)).ReturnsAsync((Form?)null);

        var result = await _controller.SubmitFormResponse(_formId, new CreateFormResponseDto { FormId = _formId });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task SubmitFormResponse_ReturnsBadRequest_WhenQuestionDoesNotBelongToForm()
    {
        var form = BuildForm();
        _mockRepository.Setup(r => r.GetFormWithQuestionsAsync(_formId)).ReturnsAsync(form);

        var dto = new CreateFormResponseDto
        {
            FormId = _formId,
            QuestionResponses =
            [
                new CreateQuestionResponseDto { QuestionId = Guid.NewGuid(), ResponseValue = "5" }
            ]
        };

        var result = await _controller.SubmitFormResponse(_formId, dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepository.Verify(r => r.SaveFormResponseAsync(It.IsAny<FormResponse>()), Times.Never);
    }

    [Fact]
    public async Task SubmitFormResponse_ReturnsBadRequest_WhenSelectedOptionDoesNotBelongToQuestion()
    {
        var form = BuildForm();
        _mockRepository.Setup(r => r.GetFormWithQuestionsAsync(_formId)).ReturnsAsync(form);

        var foreignOptionId = Guid.NewGuid(); // not one of the choice question's real options
        var dto = new CreateFormResponseDto
        {
            FormId = _formId,
            QuestionResponses =
            [
                new CreateQuestionResponseDto { QuestionId = _choiceQuestionId, SelectedOptionIds = [foreignOptionId] }
            ]
        };

        var result = await _controller.SubmitFormResponse(_formId, dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepository.Verify(r => r.SaveFormResponseAsync(It.IsAny<FormResponse>()), Times.Never);
    }

    [Fact]
    public async Task SubmitFormResponse_ReturnsBadRequest_WhenResponseValueFailsQuestionValidator()
    {
        var form = BuildForm();
        _mockRepository.Setup(r => r.GetFormWithQuestionsAsync(_formId)).ReturnsAsync(form);

        var dto = new CreateFormResponseDto
        {
            FormId = _formId,
            QuestionResponses =
            [
                new CreateQuestionResponseDto { QuestionId = _numberQuestionId, ResponseValue = "999" }, // max is 10
                new CreateQuestionResponseDto { QuestionId = _choiceQuestionId, SelectedOptionIds = [_optionAId] }
            ]
        };

        var result = await _controller.SubmitFormResponse(_formId, dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepository.Verify(r => r.SaveFormResponseAsync(It.IsAny<FormResponse>()), Times.Never);
    }

    [Fact]
    public async Task SubmitFormResponse_ReturnsBadRequest_WhenRankingSelectionCountBelowMinimum()
    {
        // Mirrors forms.md's "rank your top 3 to 5 favorites" example - 5 options, must rank 3-5
        var rankingQuestionId = Guid.NewGuid();
        var rankingQuestionType = new QuestionType { Id = Guid.NewGuid(), Code = "Ranking", Name = "Ranking", HasOptions = true };
        var options = Enumerable.Range(0, 5)
            .Select(i => new QuestionOption { Id = Guid.NewGuid(), QuestionId = rankingQuestionId, OptionText = $"Option {i}", DisplayOrder = i })
            .ToList();
        var rankingQuestion = new Question
        {
            Id = rankingQuestionId,
            FormId = _formId,
            QuestionText = "Rank your top 3 to 5 favorites",
            QuestionTypeId = rankingQuestionType.Id,
            QuestionType = rankingQuestionType,
            IsRequired = true,
            MinimumValue = "3",
            MaximumValue = "5",
            Options = options
        };
        var form = new Form { Id = _formId, Name = "Ranking Form", Questions = [rankingQuestion] };
        _mockRepository.Setup(r => r.GetFormWithQuestionsAsync(_formId)).ReturnsAsync(form);

        var dto = new CreateFormResponseDto
        {
            FormId = _formId,
            QuestionResponses =
            [
                // Only 2 ranked - below the minimum of 3
                new CreateQuestionResponseDto { QuestionId = rankingQuestionId, SelectedOptionIds = [options[0].Id, options[1].Id] }
            ]
        };

        var result = await _controller.SubmitFormResponse(_formId, dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _mockRepository.Verify(r => r.SaveFormResponseAsync(It.IsAny<FormResponse>()), Times.Never);
    }

    [Fact]
    public async Task SubmitFormResponse_ReturnsCreated_AndComputesIsCompleteTrue_WhenAllRequiredAnswered()
    {
        var form = BuildForm();
        _mockRepository.Setup(r => r.GetFormWithQuestionsAsync(_formId)).ReturnsAsync(form);

        FormResponse? saved = null;
        _mockRepository.Setup(r => r.SaveFormResponseAsync(It.IsAny<FormResponse>()))
            .ReturnsAsync((FormResponse fr) => { fr.Id = Guid.NewGuid(); saved = fr; return fr; });
        _mockRepository.Setup(r => r.GetFormResponseAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => BuildReloadedResponse(saved!, form));

        var dto = new CreateFormResponseDto
        {
            FormId = _formId,
            IsComplete = false, // deliberately wrong - server must not trust this
            QuestionResponses =
            [
                new CreateQuestionResponseDto { QuestionId = _numberQuestionId, ResponseValue = "5" },
                new CreateQuestionResponseDto { QuestionId = _choiceQuestionId, SelectedOptionIds = [_optionAId] }
            ]
        };

        var result = await _controller.SubmitFormResponse(_formId, dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var responseDto = Assert.IsType<FormResponseDto>(created.Value);
        Assert.True(responseDto.IsComplete); // all required questions answered - true regardless of client's claim
    }

    [Fact]
    public async Task SubmitFormResponse_ComputesIsCompleteFalse_WhenARequiredQuestionIsUnanswered_EvenIfClientClaimsComplete()
    {
        var form = BuildForm();
        _mockRepository.Setup(r => r.GetFormWithQuestionsAsync(_formId)).ReturnsAsync(form);

        FormResponse? saved = null;
        _mockRepository.Setup(r => r.SaveFormResponseAsync(It.IsAny<FormResponse>()))
            .ReturnsAsync((FormResponse fr) => { fr.Id = Guid.NewGuid(); saved = fr; return fr; });
        _mockRepository.Setup(r => r.GetFormResponseAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => BuildReloadedResponse(saved!, form));

        var dto = new CreateFormResponseDto
        {
            FormId = _formId,
            IsComplete = true, // deliberately wrong - server must not trust this
            QuestionResponses =
            [
                new CreateQuestionResponseDto { QuestionId = _numberQuestionId, ResponseValue = "5" }
                // choiceQuestion (required) left unanswered
            ]
        };

        var result = await _controller.SubmitFormResponse(_formId, dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var responseDto = Assert.IsType<FormResponseDto>(created.Value);
        Assert.False(responseDto.IsComplete);
    }
}
