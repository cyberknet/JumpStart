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
using System.Text.Json;
using JumpStart.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JumpStart.Data.Configuration.Forms;

/// <summary>
/// Entity Framework configuration for <see cref="QuestionType"/> including seed data.
/// </summary>
/// <remarks>
/// <para>
/// This configuration:
/// - Defines database schema constraints
/// - Seeds default question types required by the Forms module
/// - Ensures consistent data across all environments
/// </para>
/// <para>
/// Seed data is included in migrations and automatically applied during database updates.
/// No consumer action is required - the data is part of the framework's schema definition.
/// </para>
/// </remarks>
internal class QuestionTypeConfiguration : IEntityTypeConfiguration<QuestionType>
{
    /// <summary>
    /// Configures the QuestionType entity and seeds default data.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<QuestionType> builder)
    {
        // Indexes for performance
        builder.HasIndex(qt => qt.Code).IsUnique();
        builder.HasIndex(qt => qt.DisplayOrder);

                        // Seed default question types
                        builder.HasData(
                            new QuestionType
                            {
                                Id = new Guid("10000000-0000-0000-0000-000000000001"),
                                Code = "ShortText",
                                Name = "Short Text",
                                Description = "Single line text input suitable for names, email, brief answers",
                                HasOptions = false,
                                AllowsMultipleValues = false,
                                InputType = "text",
                                DisplayOrder = 1,
                                ApplicationData = JsonSerializer.Serialize(new { RazorComponentName = "ShortTextInput" })
                            },
                            new QuestionType
                            {
                                Id = new Guid("10000000-0000-0000-0000-000000000002"),
                                Code = "LongText",
                                Name = "Long Text",
                                Description = "Multi-line text area for longer responses, comments, descriptions",
                                HasOptions = false,
                                AllowsMultipleValues = false,
                                InputType = "textarea",
                                DisplayOrder = 2,
                                ApplicationData = JsonSerializer.Serialize(new { RazorComponentName = "LongTextInput" })
                            },
                            new QuestionType
                            {
                                Id = new Guid("10000000-0000-0000-0000-000000000003"),
                                Code = "Number",
                                Name = "Number",
                                Description = "Numeric input for quantities, ratings, or numeric values",
                                HasOptions = false,
                                AllowsMultipleValues = false,
                                InputType = "number",
                                DisplayOrder = 3,
                                ApplicationData = JsonSerializer.Serialize(new { RazorComponentName = "NumberInput" })
                            },
                            new QuestionType
                            {
                                Id = new Guid("10000000-0000-0000-0000-000000000004"),
                                Code = "Date",
                                Name = "Date",
                                Description = "Date picker for birth dates, appointments, event dates",
                                HasOptions = false,
                                AllowsMultipleValues = false,
                                InputType = "date",
                                DisplayOrder = 4,
                                ApplicationData = JsonSerializer.Serialize(new { RazorComponentName = "DateInput" })
                            },
                            new QuestionType
                            {
                                Id = new Guid("10000000-0000-0000-0000-000000000005"),
                                Code = "Boolean",
                                Name = "Yes/No",
                                Description = "Binary choice with Yes/No or True/False radio buttons",
                                HasOptions = false,
                                AllowsMultipleValues = false,
                                InputType = "boolean",
                                DisplayOrder = 5,
                                ApplicationData = JsonSerializer.Serialize(new { RazorComponentName = "BooleanInput" })
                            },
                            new QuestionType
                            {
                                Id = new Guid("10000000-0000-0000-0000-000000000006"),
                                Code = "SingleChoice",
                                Name = "Single Choice",
                                Description = "Radio button list allowing one option to be selected",
                                HasOptions = true,
                                AllowsMultipleValues = false,
                                InputType = "radio",
                                DisplayOrder = 6,
                                ApplicationData = JsonSerializer.Serialize(new { RazorComponentName = "SingleChoiceInput" })
                            },
                            new QuestionType
                            {
                                Id = new Guid("10000000-0000-0000-0000-000000000007"),
                                Code = "MultipleChoice",
                                Name = "Multiple Choice",
                                Description = "Checkbox list allowing multiple options to be selected",
                                HasOptions = true,
                                AllowsMultipleValues = true,
                                InputType = "checkbox",
                                DisplayOrder = 7,
                                ApplicationData = JsonSerializer.Serialize(new { RazorComponentName = "MultipleChoiceInput" })
                            },
                            new QuestionType
                            {
                                Id = new Guid("10000000-0000-0000-0000-000000000008"),
                                Code = "Dropdown",
                                Name = "Dropdown",
                                Description = "Dropdown/select list allowing one option to be selected",
                                HasOptions = true,
                                AllowsMultipleValues = false,
                                InputType = "select",
                                DisplayOrder = 8,
                                ApplicationData = JsonSerializer.Serialize(new { RazorComponentName = "DropdownInput" })
                            },
                            new QuestionType
                            {
                                Id = new Guid("10000000-0000-0000-0000-000000000009"),
                                Code = "Ranking",
                                Name = "Ranking",
                                Description = "Drag-and-drop ranking list allowing users to order options by preference",
                                HasOptions = true,
                                AllowsMultipleValues = true, // Multiple values in order
                                InputType = "ranking",
                                DisplayOrder = 9,
                                ApplicationData = JsonSerializer.Serialize(new { RazorComponentName = "RankingInput" })
                            }
                        );
                    }
                }
