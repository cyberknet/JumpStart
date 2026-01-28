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
using System.Linq;
using System.Threading.Tasks;
using JumpStart.Forms;
using Microsoft.EntityFrameworkCore;
using JumpStart.Repositories;

namespace JumpStart.Forms.Repositories;

/// <summary>
/// Repository implementation for <see cref="FormResponse"/> entities.
/// </summary>
/// <remarks>
/// Provides CRUD operations and query support for form responses.
/// </remarks>
public class FormResponseRepository : Repository<FormResponse>, IFormResponseRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormResponseRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="userContext">The user context for audit tracking (optional).</param>
    public FormResponseRepository(DbContext context, IUserContext? userContext = null)
        : base(context, userContext) { }
}
