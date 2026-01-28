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
using System.Threading.Tasks;
using JumpStart.Forms;
using JumpStart.Repositories;

namespace JumpStart.Forms.Repositories;

/// <summary>
/// Repository contract for managing <see cref="QuestionType"/> entities.
/// Inherit from this interface to provide data access for question types in forms.
/// </summary>
/// <remarks>
/// Use this interface for dependency injection and to enable unit testing of question type data access.
/// </remarks>
public interface IQuestionTypeRepository : IRepository<QuestionType>
{
}
