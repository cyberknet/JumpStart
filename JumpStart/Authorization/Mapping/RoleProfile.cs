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

using System.Linq;
using AutoMapper;
using JumpStart.Api.Mapping;
using JumpStart.Authorization.DTOs;

namespace JumpStart.Authorization.Mapping;

/// <summary>
/// AutoMapper profile for Role entity to DTO mappings.
/// </summary>
public class RoleProfile : EntityMappingProfile<Role, RoleDto, CreateRoleDto, UpdateRoleDto>
{
    /// <inheritdoc/>
    protected override void ConfigureAdditionalMappings(
        IMappingExpression<Role, RoleDto> entityMap,
        IMappingExpression<CreateRoleDto, Role> createMap,
        IMappingExpression<UpdateRoleDto, Role> updateMap)
    {
        entityMap
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions.Select(p => p.Permission)));

        createMap
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.Permissions, opt => opt.Ignore())
            .ForMember(dest => dest.UserAssignments, opt => opt.Ignore());

        updateMap
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.Permissions, opt => opt.Ignore())
            .ForMember(dest => dest.UserAssignments, opt => opt.Ignore());
    }
}
