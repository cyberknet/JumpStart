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
using JumpStart.Api.Mapping;
using JumpStart.Forms;
using JumpStart.Forms.DTOs;

namespace JumpStart.Forms.Mapping;

/// <summary>
/// AutoMapper profile for QuestionResponse entity to DTO mappings.
/// </summary>
/// <remarks>
/// Configures mappings between <see cref="QuestionResponse"/>, <see cref="QuestionResponseDto"/>, <see cref="QuestionResponseDto"/>.
/// </remarks>
public class QuestionResponseProfile : EntityMappingProfile<QuestionResponse, QuestionResponseDto, CreateQuestionResponseDto, UpdateQuestionResponseDto>
{
    /// <inheritdoc/>
    protected override void ConfigureAdditionalMappings(IMappingExpression<QuestionResponse, QuestionResponseDto> entityMap, IMappingExpression<CreateQuestionResponseDto, QuestionResponse> createMap, IMappingExpression<UpdateQuestionResponseDto, QuestionResponse> updateMap)
    {
        entityMap
            .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.QuestionText))
            .ForMember(dest => dest.ResponseValue, opt => opt.MapFrom(src => src.ResponseText))
            .ForMember(dest => dest.SelectedOptions, opt => opt.MapFrom(src => src.SelectedOptions.Select(so => so.QuestionOption.OptionText).ToList()));

        createMap
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FormResponseId, opt => opt.Ignore())
            .ForMember(dest => dest.FormResponse, opt => opt.Ignore())
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseText, opt => opt.MapFrom(src => src.ResponseValue))
            .ForMember(dest => dest.SelectedOptions, opt => opt.MapFrom(src => src.SelectedOptionIds.Select(id => new QuestionResponseOption { QuestionOptionId = id })));

        updateMap
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FormResponseId, opt => opt.Ignore())
            .ForMember(dest => dest.FormResponse, opt => opt.Ignore())
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseText, opt => opt.MapFrom(src => src.ResponseValue))
            .ForMember(dest => dest.SelectedOptions, opt => opt.MapFrom(src => src.SelectedOptionIds.Select(id => new QuestionResponseOption { QuestionOptionId = id })));
    }
}
