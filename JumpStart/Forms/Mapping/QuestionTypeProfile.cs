using AutoMapper;
using JumpStart.Forms.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Forms.Mapping;

/// <summary>
/// AutoMapper profile for QuestionType entity to DTO mappings.
/// </summary>
/// <remarks>
/// Configures mappings between <see cref="QuestionType"/>, <see cref="QuestionTypeDto"/>, <see cref="QuestionTypeDto"/>.
/// </remarks>
public class QuestionTypeProfile : Profile
{
    /// <summary>
    /// Configures mappings for QuestionType entity.
    /// </summary>
    public QuestionTypeProfile()
	{
        // QuestionType mappings
        CreateMap<QuestionType, QuestionTypeDto>();
        CreateMap<CreateQuestionTypeDto, QuestionType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.Ignore());
    }
}
