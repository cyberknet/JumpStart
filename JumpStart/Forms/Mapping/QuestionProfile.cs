using AutoMapper;
using JumpStart.Api.Mapping;
using JumpStart.Forms.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Forms.Mapping;

/// <summary>
/// AutoMapper profile for Question entity to DTO mappings.
/// </summary>
/// <remarks>
/// Configures mappings between <see cref="Question"/>, <see cref="QuestionDto"/>, <see cref="QuestionDto"/>.
/// </remarks>
public class QuestionProfile : EntityMappingProfile<Question, QuestionDto, CreateQuestionDto, UpdateQuestionDto>
{
    protected override void ConfigureAdditionalMappings(IMappingExpression<Question, QuestionDto> entityMap, IMappingExpression<CreateQuestionDto, Question> createMap, IMappingExpression<UpdateQuestionDto, Question> updateMap)
    {
        entityMap
            .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.QuestionType))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        createMap
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FormId, opt => opt.Ignore())
            .ForMember(dest => dest.Form, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionType, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore())
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        updateMap
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FormId, opt => opt.Ignore())
            .ForMember(dest => dest.Form, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionType, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore())
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));
    }
}
