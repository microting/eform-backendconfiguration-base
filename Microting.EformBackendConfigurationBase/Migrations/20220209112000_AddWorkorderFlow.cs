using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddWorkorderFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderIdForTasks",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WorkorderEnable",
                table: "PropertieVersions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FolderIdForTasks",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WorkorderEnable",
                table: "Properties",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WorkorderCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PropertyWorkerId = table.Column<int>(type: "int", nullable: false),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkorderCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkorderCases_PropertyWorkers_PropertyWorkerId",
                        column: x => x.PropertyWorkerId,
                        principalTable: "PropertyWorkers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkorderCaseVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkorderCaseId = table.Column<int>(type: "int", nullable: false),
                    PropertyWorkerId = table.Column<int>(type: "int", nullable: false),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkorderCaseVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WorkorderCases_PropertyWorkerId",
                table: "WorkorderCases",
                column: "PropertyWorkerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkorderCases");

            migrationBuilder.DropTable(
                name: "WorkorderCaseVersions");

            migrationBuilder.DropColumn(
                name: "FolderIdForTasks",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "WorkorderEnable",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "FolderIdForTasks",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "WorkorderEnable",
                table: "Properties");
        }
    }
}
