using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddingIndexOnCompliancePlanningIdAndDeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create a new table without duplicates
            migrationBuilder.CreateTable(
                name: "TempCompliances",
                columns: table => new
                {
                    ItemName = table.Column<string>(nullable: true),
                    AreaId = table.Column<int>(nullable: false),
                    AreaName = table.Column<string>(nullable: true),
                    PlanningId = table.Column<int>(nullable: false),
                    PropertyId = table.Column<int>(nullable: false),
                    Deadline = table.Column<DateTime>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    MicrotingSdkCaseId = table.Column<int>(nullable: false),
                    MicrotingSdkeFormId = table.Column<int>(nullable: false),
                    PlanningCaseSiteId = table.Column<int>(nullable: false),
                    CheckListSiteId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempCompliances", x => new { x.PlanningId, x.Deadline });
                });

            // Step 2: Insert distinct rows into the new table
            migrationBuilder.Sql(@"
        INSERT INTO TempCompliances
        SELECT DISTINCT *
        FROM Compliances");

            // Step 3: Delete the original table
            migrationBuilder.DropTable("Compliances");

            // Step 4: Rename the new table to the original table name
            migrationBuilder.RenameTable("TempCompliances", newName: "Compliances");

            // Step 5: Add the unique index to the new table
            migrationBuilder.CreateIndex(
                name: "IX_PlanningId_Deadline",
                table: "Compliances",
                columns: new[] { "PlanningId", "Deadline" },
                unique: true);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Compliances_PlanningId_Deadline",
                table: "Compliances");
        }
    }
}