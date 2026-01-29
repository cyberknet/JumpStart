using AutoMapper;
using JumpStart.Api.Mapping;
using JumpStart.Forms.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Forms.Mapping;

/// <summary>
/// AutoMapper profile for QuestionOption entity to DTO mappings.
/// </summary>
/// <remarks>
/// Configures mappings between <see cref="QuestionOption"/>, <see cref="QuestionOptionDto"/>, <see cref="QuestionOptionDto"/>.
/// </remarks>
public class QuestionOptionProfile : EntityMappingProfile<QuestionOption, QuestionOptionDto, CreateQuestionOptionDto, UpdateQuestionOptionDto>
{
    /// <inheritdoc/>
    override protected void ConfigureAdditionalMappings(IMappingExpression<QuestionOption, QuestionOptionDto> entityMap, IMappingExpression<CreateQuestionOptionDto, QuestionOption> createMap, IMappingExpression<UpdateQuestionOptionDto, QuestionOption> updateMap)
    {
        entityMap
            .ReverseMap();

        createMap
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseSelections, opt => opt.Ignore());

        updateMap
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseSelections, opt => opt.Ignore());
    }
}
