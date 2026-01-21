using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JumpStart.DemoApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRankingQuestionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "QuestionTypes",
                columns: new[] { "Id", "AllowsMultipleValues", "ApplicationData", "Code", "Description", "DisplayOrder", "HasOptions", "InputType", "Name" },
                values: new object[] { new Guid("10000000-0000-0000-0000-000000000009"), true, "{\"RazorComponentName\":\"RankingInput\"}", "Ranking", "Drag-and-drop ranking list allowing users to order options by preference", 9, true, "ranking", "Ranking" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QuestionTypes",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));
        }
    }
}
