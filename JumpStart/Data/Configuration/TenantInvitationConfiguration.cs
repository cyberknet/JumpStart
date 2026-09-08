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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JumpStart.Data.Configuration;

/// <summary>
/// Schema for <see cref="TenantInvitation"/>.
/// </summary>
/// <remarks>
/// Picked up automatically by <c>ApplyConfigurationsFromAssembly</c>, so the constraints below travel
/// with the entity rather than needing to be re-stated by every application that uses it. The unique
/// index on the token is the load-bearing one: it is a credential, and a duplicate would make
/// redemption ambiguous.
/// </remarks>
public class TenantInvitationConfiguration : IEntityTypeConfiguration<TenantInvitation>
{
    public void Configure(EntityTypeBuilder<TenantInvitation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(i => i.Email).IsRequired().HasMaxLength(320);
        builder.Property(i => i.Token).IsRequired().HasMaxLength(64);

        builder.HasIndex(i => i.Token).IsUnique();

        // The listing query: outstanding invitations for one tenant, and the "is this address already
        // invited?" check that precedes issuing a new one.
        builder.HasIndex(i => new { i.TenantId, i.Email });
    }
}
