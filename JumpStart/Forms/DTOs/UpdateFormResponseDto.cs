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

using JumpStart.Api.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JumpStart.Forms.DTOs;


public class UpdateFormResponseDto : EntityDto, IUpdateDto
{
    /// <summary>Gets or sets the form ID being responded to.</summary>
    [Required]
    public Guid FormId { get; set; }
    
    /// <summary>Gets or sets the user ID of the respondent (null for anonymous).</summary>
    public Guid? RespondentUserId { get; set; }
    
    /// <summary>Gets or sets whether this response is complete.</summary>
    public bool IsComplete { get; set; }
    
    /// <summary>Gets or sets the question responses.</summary>
    [Required]
    public List<CreateQuestionResponseDto> QuestionResponses { get; set; } = new();
}
