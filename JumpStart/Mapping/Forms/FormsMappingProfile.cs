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
using JumpStart.Api.DTOs.Forms;
using JumpStart.Forms;

namespace JumpStart.Mapping.Forms;

/// <summary>
/// AutoMapper profile for Forms module entity-DTO mappings.
/// </summary>
/// <remarks>
/// <para>
/// This profile configures bidirectional mappings between Forms entities and their DTOs,
/// enabling clean separation between domain models and API contracts.
/// </para>
/// <para>
/// <strong>Mapping Strategies:</strong>
/// - Two-way mappings for CRUD operations (Form ↔ FormDto)
/// - Create DTOs map only to entities (CreateFormDto → Form)
/// - Update DTOs use ReverseMap for patching existing entities
/// - Complex nested mappings (Form → FormWithQuestionsDto with questions and options)
/// - Enum ↔ String conversions for QuestionType
/// </para>
/// <para>
/// <strong>Registration:</strong>
/// This profile is automatically discovered and registered when using:
/// <code>
/// services.AddJumpStartAutoMapper(typeof(Program).Assembly);
/// </code>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In a controller or service
/// public class FormsService
/// {
///     private readonly IFormRepository _repository;
///     private readonly IMapper _mapper;
///     
///     public async Task&lt;FormDto&gt; CreateFormAsync(CreateFormDto createDto)
///     {
///         // DTO → Entity
///         var form = _mapper.Map&lt;Form&gt;(createDto);
///         await _repository.AddAsync(form);
///         
///         // Entity → DTO
///         return _mapper.Map&lt;FormDto&gt;(form);
///     }
///     
///     public async Task&lt;FormWithQuestionsDto&gt; GetFormWithQuestionsAsync(Guid id)
///     {
///         var form = await _repository.GetFormWithQuestionsAsync(id);
///         
///         // Entity → DTO (includes nested questions and options)
///         return _mapper.Map&lt;FormWithQuestionsDto&gt;(form);
///     }
/// }
/// </code>
/// </example>
public class FormsMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormsMappingProfile"/> class.
    /// Configures all Forms entity-DTO mappings.
    /// </summary>
    public FormsMappingProfile()
    {
        // QuestionType mappings
        CreateMap<QuestionType, QuestionTypeDto>();

        // Form mappings
        CreateMap<Form, FormDto>()
            .ReverseMap();

        CreateMap<Form, FormWithQuestionsDto>()
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

        CreateMap<CreateFormDto, Form>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

        CreateMap<UpdateFormDto, Form>()
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.Ignore());

        // Question mappings
        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.QuestionType))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        CreateMap<CreateQuestionDto, Question>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FormId, opt => opt.Ignore())
            .ForMember(dest => dest.Form, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionType, opt => opt.Ignore())
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        // QuestionOption mappings
        CreateMap<QuestionOption, QuestionOptionDto>()
            .ReverseMap();

        CreateMap<CreateQuestionOptionDto, QuestionOption>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseSelections, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedById, opt => opt.Ignore());
    }
}
