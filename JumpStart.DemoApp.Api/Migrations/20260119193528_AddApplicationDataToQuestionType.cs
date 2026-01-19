using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JumpStart.DemoApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationDataToQuestionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationData",
                table: "QuestionTypes",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "QuestionTypes",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "ApplicationData",
                value: "{\"RazorComponentName\":\"ShortTextInput\"}");

            migrationBuilder.UpdateData(
                table: "QuestionTypes",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "ApplicationData",
                value: "{\"RazorComponentName\":\"LongTextInput\"}");

            migrationBuilder.UpdateData(
                table: "QuestionTypes",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "ApplicationData",
                value: "{\"RazorComponentName\":\"NumberInput\"}");

            migrationBuilder.UpdateData(
                table: "QuestionTypes",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "ApplicationData",
                value: "{\"RazorComponentName\":\"DateInput\"}");

            migrationBuilder.UpdateData(
                table: "QuestionTypes",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "ApplicationData",
                value: "{\"RazorComponentName\":\"BooleanInput\"}");

            migrationBuilder.UpdateData(
                table: "QuestionTypes",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                column: "ApplicationData",
                value: "{\"RazorComponentName\":\"SingleChoiceInput\"}");

            migrationBuilder.UpdateData(
                table: "QuestionTypes",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                column: "ApplicationData",
                value: "{\"RazorComponentName\":\"MultipleChoiceInput\"}");

            migrationBuilder.UpdateData(
                table: "QuestionTypes",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                column: "ApplicationData",
                value: "{\"RazorComponentName\":\"DropdownInput\"}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationData",
                table: "QuestionTypes");
        }
    }
}
