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
/// AutoMapper profile for FormResponse entity to DTO mappings.
/// </summary>
/// <remarks>
/// Configures mappings between <see cref="FormResponse"/>, <see cref="FormResponseDto"/>
/// </remarks>
public class FormResponseProfile : EntityMappingProfile<FormResponse, FormResponseDto, CreateFormResponseDto, UpdateFormResponseDto>
{
    /// <inheritdoc/>
    protected override void ConfigureAdditionalMappings(IMappingExpression<FormResponse, FormResponseDto> entityMap, IMappingExpression<CreateFormResponseDto, FormResponse> createMap, IMappingExpression<UpdateFormResponseDto, FormResponse> updateMap)
    {
        entityMap
            .ForMember(dest => dest.FormName, opt => opt.MapFrom(src => src.Form.Name))
            .ForMember(dest => dest.SubmittedOn, opt => opt.MapFrom(src => src.SubmittedOn))
            .ForMember(dest => dest.QuestionResponses, opt => opt.MapFrom(src => src.Answers));

        createMap
            .ForMember(dest => dest.SubmittedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Form, opt => opt.Ignore())
            .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.QuestionResponses));
        updateMap
            .ForMember(dest => dest.SubmittedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Form, opt => opt.Ignore())
            .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.QuestionResponses));
    }
}
