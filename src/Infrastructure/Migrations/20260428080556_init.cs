using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Checklist");

            migrationBuilder.CreateTable(
                name: "Checklists",
                schema: "Checklist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checklists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                schema: "Checklist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistVersion = table.Column<int>(type: "int", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TotalScore = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalSchema: "Checklist",
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistGroup",
                schema: "Checklist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChecklistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsShow = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistGroup_ChecklistGroup_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Checklist",
                        principalTable: "ChecklistGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChecklistGroup_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalSchema: "Checklist",
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistQuestion",
                schema: "Checklist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<float>(type: "real", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsRequiredAnswer = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistQuestion_ChecklistGroup_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Checklist",
                        principalTable: "ChecklistGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentAnswer",
                schema: "Checklist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectedOptionIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentAnswer_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Checklist",
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentAnswer_ChecklistQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "Checklist",
                        principalTable: "ChecklistQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistQuestionOption",
                schema: "Checklist",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistQuestionOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistQuestionOption_ChecklistQuestion_ChecklistQuestionId",
                        column: x => x.ChecklistQuestionId,
                        principalSchema: "Checklist",
                        principalTable: "ChecklistQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAnswer_AssessmentId",
                schema: "Checklist",
                table: "AssessmentAnswer",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAnswer_CreatedAt",
                schema: "Checklist",
                table: "AssessmentAnswer",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAnswer_IsDeleted",
                schema: "Checklist",
                table: "AssessmentAnswer",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAnswer_QuestionId",
                schema: "Checklist",
                table: "AssessmentAnswer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAnswer_UpdatedAt",
                schema: "Checklist",
                table: "AssessmentAnswer",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_AssessmentDate",
                schema: "Checklist",
                table: "Assessments",
                column: "AssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_ChecklistId",
                schema: "Checklist",
                table: "Assessments",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_ChecklistVersion",
                schema: "Checklist",
                table: "Assessments",
                column: "ChecklistVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CreatedAt",
                schema: "Checklist",
                table: "Assessments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_IsDeleted",
                schema: "Checklist",
                table: "Assessments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_UpdatedAt",
                schema: "Checklist",
                table: "Assessments",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistGroup_ChecklistId",
                schema: "Checklist",
                table: "ChecklistGroup",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistGroup_CreatedAt",
                schema: "Checklist",
                table: "ChecklistGroup",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistGroup_IsDeleted",
                schema: "Checklist",
                table: "ChecklistGroup",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistGroup_ParentId",
                schema: "Checklist",
                table: "ChecklistGroup",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistGroup_UpdatedAt",
                schema: "Checklist",
                table: "ChecklistGroup",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestion_CreatedAt",
                schema: "Checklist",
                table: "ChecklistQuestion",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestion_GroupId",
                schema: "Checklist",
                table: "ChecklistQuestion",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestion_IsDeleted",
                schema: "Checklist",
                table: "ChecklistQuestion",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestion_UpdatedAt",
                schema: "Checklist",
                table: "ChecklistQuestion",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestionOption_ChecklistQuestionId",
                schema: "Checklist",
                table: "ChecklistQuestionOption",
                column: "ChecklistQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestionOption_CreatedAt",
                schema: "Checklist",
                table: "ChecklistQuestionOption",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestionOption_IsDeleted",
                schema: "Checklist",
                table: "ChecklistQuestionOption",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestionOption_UpdatedAt",
                schema: "Checklist",
                table: "ChecklistQuestionOption",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_CreatedAt",
                schema: "Checklist",
                table: "Checklists",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_IsDeleted",
                schema: "Checklist",
                table: "Checklists",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_Title",
                schema: "Checklist",
                table: "Checklists",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_UpdatedAt",
                schema: "Checklist",
                table: "Checklists",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentAnswer",
                schema: "Checklist");

            migrationBuilder.DropTable(
                name: "ChecklistQuestionOption",
                schema: "Checklist");

            migrationBuilder.DropTable(
                name: "Assessments",
                schema: "Checklist");

            migrationBuilder.DropTable(
                name: "ChecklistQuestion",
                schema: "Checklist");

            migrationBuilder.DropTable(
                name: "ChecklistGroup",
                schema: "Checklist");

            migrationBuilder.DropTable(
                name: "Checklists",
                schema: "Checklist");
        }
    }
}
