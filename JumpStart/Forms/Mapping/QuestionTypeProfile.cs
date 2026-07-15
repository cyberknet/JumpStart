using AutoMapper;
using JumpStart.Api.Mapping;
using JumpStart.Forms.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Forms.Mapping;

/// <summary>
/// AutoMapper profile for QuestionType entity to DTO mappings.
/// </summary>
/// <remarks>
/// Configures mappings between <see cref="QuestionType"/>, <see cref="QuestionTypeDto"/>,
/// <see cref="CreateQuestionTypeDto"/>, and <see cref="UpdateQuestionTypeDto"/>.
/// </remarks>
public class QuestionTypeProfile : EntityMappingProfile<QuestionType, QuestionTypeDto, CreateQuestionTypeDto, UpdateQuestionTypeDto>
{
    /// <inheritdoc/>
    protected override void ConfigureAdditionalMappings(IMappingExpression<QuestionType, QuestionTypeDto> entityMap, IMappingExpression<CreateQuestionTypeDto, QuestionType> createMap, IMappingExpression<UpdateQuestionTypeDto, QuestionType> updateMap)
    {
        createMap
            .ForMember(dest => dest.Questions, opt => opt.Ignore());
        updateMap
            .ForMember(dest => dest.Questions, opt => opt.Ignore());
    }

}
