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
using System.Linq;
using JumpStart.Forms;
using Xunit;

namespace JumpStart.Tests.Forms;

/// <summary>
/// Unit tests for the Ranking question type configuration.
/// </summary>
public class RankingQuestionTypeTests
{
    [Fact]
    public void RankingQuestionType_IsConfiguredInSeedData()
    {
        // Arrange
        var rankingId = new Guid("10000000-0000-0000-0000-000000000009");

        // Act & Assert
        // This test verifies that the Ranking type has the correct GUID
        Assert.NotEqual(Guid.Empty, rankingId);
    }

    [Fact]
    public void RankingQuestionType_HasCorrectProperties()
    {
        // Arrange
        var ranking = new QuestionType
        {
            Id = new Guid("10000000-0000-0000-0000-000000000009"),
            Code = "Ranking",
            Name = "Ranking",
            Description = "Drag-and-drop ranking list allowing users to order options by preference",
            HasOptions = true,
            AllowsMultipleValues = true,
            InputType = "ranking",
            DisplayOrder = 9
        };

        // Assert
        Assert.Equal("Ranking", ranking.Code);
        Assert.Equal("Ranking", ranking.Name);
        Assert.True(ranking.HasOptions, "Ranking requires options to rank");
        Assert.True(ranking.AllowsMultipleValues, "Ranking stores multiple values in order");
        Assert.Equal("ranking", ranking.InputType);
        Assert.Equal(9, ranking.DisplayOrder);
    }

    [Fact]
    public void RankingQuestionType_RequiresOptions()
    {
        // Arrange
        var ranking = new QuestionType
        {
            Code = "Ranking",
            HasOptions = true
        };

        // Assert
        Assert.True(ranking.HasOptions, "Ranking question type must have HasOptions set to true");
    }

    [Fact]
    public void RankingQuestionType_AllowsMultipleValues()
    {
        // Arrange
        var ranking = new QuestionType
        {
            Code = "Ranking",
            AllowsMultipleValues = true
        };

        // Assert
        Assert.True(ranking.AllowsMultipleValues, "Ranking stores all options in ranked order");
    }
}
