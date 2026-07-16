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

using JumpStart.Data;
using JumpStart.Repositories;

namespace JumpStart.MultiTenant.Repositories;

/// <summary>
/// Repository interface for managing <see cref="Tenant"/> entities. See ADR-015.
/// </summary>
/// <remarks>
/// Standard CRUD only - <see cref="Tenant"/> itself has no tenant-specific query needs beyond what
/// <see cref="IRepository{TEntity}"/> already provides. See <see cref="IUserTenantRepository"/> for
/// user-tenant membership queries.
/// </remarks>
public interface ITenantRepository : IRepository<Tenant>
{
}
