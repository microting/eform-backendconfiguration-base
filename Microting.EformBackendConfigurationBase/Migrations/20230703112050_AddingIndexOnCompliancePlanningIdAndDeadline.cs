using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddingIndexOnCompliancePlanningIdAndDeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"DELETE FROM Compliances
            WHERE Id IN (
                SELECT duplicate.Id
                FROM (
                    SELECT MIN(Id) AS Id
                    FROM Compliances
                    GROUP BY PlanningId, Deadline
                    HAVING COUNT(*) > 1
                ) AS duplicate
            )");

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