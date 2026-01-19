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

using System;
using System.Collections.Generic;

namespace JumpStart.Api.DTOs.Forms;

/// <summary>
/// Data transfer object for a saved form response.
/// </summary>
public class FormResponseDto
{
    /// <summary>Gets or sets the response ID.</summary>
    public Guid Id { get; set; }
    
    /// <summary>Gets or sets the form ID.</summary>
    public Guid FormId { get; set; }
    
    /// <summary>Gets or sets the form name.</summary>
    public string FormName { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the respondent user ID.</summary>
    public Guid? RespondentUserId { get; set; }
    
    /// <summary>Gets or sets when the response was submitted.</summary>
    public DateTime SubmittedOn { get; set; }
    
    /// <summary>Gets or sets whether the response is complete.</summary>
    public bool IsComplete { get; set; }
    
    /// <summary>Gets or sets the question responses.</summary>
    public List<QuestionResponseDto> QuestionResponses { get; set; } = new();
}

/// <summary>
/// Data transfer object for a question response.
/// </summary>
public class QuestionResponseDto
{
    /// <summary>Gets or sets the question response ID.</summary>
    public Guid Id { get; set; }
    
    /// <summary>Gets or sets the question ID.</summary>
    public Guid QuestionId { get; set; }
    
    /// <summary>Gets or sets the question text.</summary>
    public string QuestionText { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the response value.</summary>
    public string? ResponseValue { get; set; }
    
    /// <summary>Gets or sets the selected option texts.</summary>
    public List<string> SelectedOptions { get; set; } = new();
}
