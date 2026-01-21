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

namespace JumpStart.Api.Mapping;

/// <summary>
/// AutoMapper profile for Forms module entity to DTO mappings.
/// </summary>
public class FormsProfile : Profile
{
    /// <summary>
    /// Configures mappings for Forms entities.
    /// </summary>
    public FormsProfile()
    {
        // QuestionType mappings
        CreateMap<QuestionType, QuestionTypeDto>();
        CreateMap<CreateQuestionTypeDto, QuestionType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.Ignore());

        // Form mappings
        CreateMap<Form, FormDto>()
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn.DateTime));

        CreateMap<CreateFormDto, Form>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore());
        
        CreateMap<UpdateFormDto, Form>()
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore());
        
        // Question mappings
        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.QuestionType));
        
        CreateMap<CreateQuestionDto, Question>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FormId, opt => opt.Ignore())
            .ForMember(dest => dest.Form, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionType, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore());
        
        // QuestionOption mappings
        CreateMap<QuestionOption, QuestionOptionDto>();

        CreateMap<CreateQuestionOptionDto, QuestionOption>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ForMember(dest => dest.ResponseSelections, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedById, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore());
        
                                // FormWithQuestions mapping
                                CreateMap<Form, FormWithQuestionsDto>()
                                    .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

                                        // FormResponse mappings
                                        CreateMap<CreateFormResponseDto, FormResponse>()
                                            .ForMember(dest => dest.Id, opt => opt.Ignore())
                                            .ForMember(dest => dest.SubmittedOn, opt => opt.Ignore())
                                            .ForMember(dest => dest.Form, opt => opt.Ignore())
                                            .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.QuestionResponses))
                                            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
                                            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                                            .ForMember(dest => dest.ModifiedById, opt => opt.Ignore())
                                            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                                            .ForMember(dest => dest.DeletedById, opt => opt.Ignore())
                                            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore());

                                        CreateMap<CreateQuestionResponseDto, QuestionResponse>()
                                            .ForMember(dest => dest.Id, opt => opt.Ignore())
                                            .ForMember(dest => dest.FormResponseId, opt => opt.Ignore())
                                            .ForMember(dest => dest.FormResponse, opt => opt.Ignore())
                                            .ForMember(dest => dest.Question, opt => opt.Ignore())
                                            .ForMember(dest => dest.ResponseText, opt => opt.MapFrom(src => src.ResponseValue))
                                            .ForMember(dest => dest.SelectedOptions, opt => opt.MapFrom(src =>
                                                src.SelectedOptionIds.Select(id => new QuestionResponseOption { QuestionOptionId = id })));

                                        CreateMap<FormResponse, FormResponseDto>()
                                            .ForMember(dest => dest.FormName, opt => opt.MapFrom(src => src.Form.Name))
                                            .ForMember(dest => dest.SubmittedOn, opt => opt.MapFrom(src => src.SubmittedOn))
                                            .ForMember(dest => dest.QuestionResponses, opt => opt.MapFrom(src => src.Answers));

                                        CreateMap<QuestionResponse, QuestionResponseDto>()
                                            .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.QuestionText))
                                            .ForMember(dest => dest.ResponseValue, opt => opt.MapFrom(src => src.ResponseText))
                                            .ForMember(dest => dest.SelectedOptions, opt => opt.MapFrom(src =>
                                                src.SelectedOptions.Select(so => so.QuestionOption.OptionText).ToList()));
                                    }
                                }
