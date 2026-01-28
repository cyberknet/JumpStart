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
/// AutoMapper profile for Form entity to DTO mappings.
/// </summary>
/// <remarks>
/// Configures mappings between <see cref="Form"/>, <see cref="FormDto"/>
/// </remarks>
public class FormProfile : EntityMappingProfile<Form, FormDto, CreateFormDto, UpdateFormDto>
{
    protected override void ConfigureAdditionalMappings(IMappingExpression<Form, FormDto> entityMap, IMappingExpression<CreateFormDto, Form> createMap, IMappingExpression<UpdateFormDto, Form> updateMap)
    {
        entityMap
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn.DateTime))
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

        createMap
            .ForMember(dest => dest.Responses, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

        updateMap
            .ForMember(dest => dest.Responses, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.Ignore());
    }
}
