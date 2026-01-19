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
/// Entity Framework configuration for QuestionResponseOption.
/// </summary>
internal class QuestionResponseOptionConfiguration : IEntityTypeConfiguration<QuestionResponseOption>
{
    public void Configure(EntityTypeBuilder<QuestionResponseOption> builder)
    {
        // Relationship with QuestionOption - use Restrict to avoid cascade cycles
        builder.HasOne(qro => qro.QuestionOption)
            .WithMany()
            .HasForeignKey(qro => qro.QuestionOptionId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Relationship with QuestionResponse - use Restrict to avoid cascade cycles
        builder.HasOne(qro => qro.QuestionResponse)
            .WithMany(qr => qr.SelectedOptions)
            .HasForeignKey(qro => qro.QuestionResponseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
