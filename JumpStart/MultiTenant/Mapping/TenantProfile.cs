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
using JumpStart.Data;
using JumpStart.MultiTenant.DTOs;

namespace JumpStart.MultiTenant.Mapping;

/// <summary>
/// AutoMapper profile for Tenant entity to DTO mappings.
/// </summary>
public class TenantProfile : EntityMappingProfile<Tenant, TenantDto, CreateTenantDto, UpdateTenantDto>
{
    /// <inheritdoc/>
    protected override void ConfigureAdditionalMappings(
        IMappingExpression<Tenant, TenantDto> entityMap,
        IMappingExpression<CreateTenantDto, Tenant> createMap,
        IMappingExpression<UpdateTenantDto, Tenant> updateMap)
    {
        createMap
            .ForMember(dest => dest.UserTenants, opt => opt.Ignore())
            .ForMember(dest => dest.Settings, opt => opt.Ignore());

        updateMap
            .ForMember(dest => dest.UserTenants, opt => opt.Ignore())
            .ForMember(dest => dest.Settings, opt => opt.Ignore());
    }
}
