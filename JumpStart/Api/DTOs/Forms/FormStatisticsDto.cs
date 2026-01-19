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

namespace JumpStart.Api.DTOs.Forms;

/// <summary>
/// Data transfer object for form statistics.
/// </summary>
public class FormStatisticsDto
{
    /// <summary>Gets or sets the form ID.</summary>
    public Guid FormId { get; set; }
    
    /// <summary>Gets or sets the form name.</summary>
    public string FormName { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the total number of responses.</summary>
    public int TotalResponses { get; set; }
    
    /// <summary>Gets or sets the number of complete responses.</summary>
    public int CompleteResponses { get; set; }
    
    /// <summary>Gets or sets the number of incomplete responses.</summary>
    public int IncompleteResponses { get; set; }
    
    /// <summary>Gets or sets the completion rate as a percentage (0-100).</summary>
    public double CompletionRate { get; set; }
}
