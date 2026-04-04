using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistGroup_Checklists_ChecklistId",
                schema: "dbo",
                table: "ChecklistGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistQuestion_Checklists_ChecklistId",
                schema: "dbo",
                table: "ChecklistQuestion");

            migrationBuilder.DropIndex(
                name: "IX_ChecklistQuestion_ChecklistId",
                schema: "dbo",
                table: "ChecklistQuestion");

            migrationBuilder.DropColumn(
                name: "ChecklistId",
                schema: "dbo",
                table: "ChecklistQuestion");

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionId",
                schema: "dbo",
                table: "ChecklistQuestionOption",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistGroup_Checklists_ChecklistId",
                schema: "dbo",
                table: "ChecklistGroup",
                column: "ChecklistId",
                principalSchema: "dbo",
                principalTable: "Checklists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChecklistGroup_Checklists_ChecklistId",
                schema: "dbo",
                table: "ChecklistGroup");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                schema: "dbo",
                table: "ChecklistQuestionOption");

            migrationBuilder.AddColumn<Guid>(
                name: "ChecklistId",
                schema: "dbo",
                table: "ChecklistQuestion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestion_ChecklistId",
                schema: "dbo",
                table: "ChecklistQuestion",
                column: "ChecklistId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistGroup_Checklists_ChecklistId",
                schema: "dbo",
                table: "ChecklistGroup",
                column: "ChecklistId",
                principalSchema: "dbo",
                principalTable: "Checklists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistQuestion_Checklists_ChecklistId",
                schema: "dbo",
                table: "ChecklistQuestion",
                column: "ChecklistId",
                principalSchema: "dbo",
                principalTable: "Checklists",
                principalColumn: "Id");
        }
    }
}
