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

using JumpStart.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JumpStart.Data.Configuration.Forms;

/// <summary>
/// Entity Framework configuration for QuestionResponse.
/// </summary>
internal class QuestionResponseConfiguration : IEntityTypeConfiguration<QuestionResponse>
{
    public void Configure(EntityTypeBuilder<QuestionResponse> builder)
    {
        // Relationship with Question - use Restrict to avoid cascade cycles
        builder.HasOne(qr => qr.Question)
            .WithMany(q => q.Responses)
            .HasForeignKey(qr => qr.QuestionId)
            .OnDelete(DeleteBehavior.Restrict); // ← Prevents cascade cycle
    }
}
