using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddLinksAndNewFieldForAreaRulePlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemPlanningTagId",
                table: "AreaRulesPlanningVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemPlanningTagId",
                table: "AreaRulePlannings",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AreaRulePlannings_Areas_AreaId",
                table: "AreaRulePlannings");

            migrationBuilder.DropForeignKey(
                name: "FK_AreaRulePlannings_Properties_PropertyId",
                table: "AreaRulePlannings");

            migrationBuilder.DropForeignKey(
                name: "FK_ChemicalProductProperties_Properties_PropertyId",
                table: "ChemicalProductProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_ChemicalProductPropertieSites_Properties_PropertyId",
                table: "ChemicalProductPropertieSites");

            migrationBuilder.DropForeignKey(
                name: "FK_Compliances_Areas_AreaId",
                table: "Compliances");

            migrationBuilder.DropForeignKey(
                name: "FK_Compliances_Properties_PropertyId",
                table: "Compliances");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningSites_AreaRules_AreaRuleId",
                table: "PlanningSites");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningSites_Areas_AreaId",
                table: "PlanningSites");

            migrationBuilder.DropForeignKey(
                name: "FK_PoolAccidents_AreaRules_AreaRuleId",
                table: "PoolAccidents");

            migrationBuilder.DropForeignKey(
                name: "FK_PoolHistorySites_AreaRules_AreaRuleId",
                table: "PoolHistorySites");

            migrationBuilder.DropForeignKey(
                name: "FK_PoolHours_AreaRules_AreaRuleId",
                table: "PoolHours");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkorderCaseImages_UploadedDatas_UploadedDataId",
                table: "WorkorderCaseImages");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkorderCaseImages_WorkorderCases_WorkorderCaseId",
                table: "WorkorderCaseImages");

            migrationBuilder.DropIndex(
                name: "IX_WorkorderCaseImages_UploadedDataId",
                table: "WorkorderCaseImages");

            migrationBuilder.DropIndex(
                name: "IX_WorkorderCaseImages_WorkorderCaseId",
                table: "WorkorderCaseImages");

            migrationBuilder.DropIndex(
                name: "IX_PoolHours_AreaRuleId",
                table: "PoolHours");

            migrationBuilder.DropIndex(
                name: "IX_PoolHistorySites_AreaRuleId",
                table: "PoolHistorySites");

            migrationBuilder.DropIndex(
                name: "IX_PoolAccidents_AreaRuleId",
                table: "PoolAccidents");

            migrationBuilder.DropIndex(
                name: "IX_PlanningSites_AreaId",
                table: "PlanningSites");

            migrationBuilder.DropIndex(
                name: "IX_PlanningSites_AreaRuleId",
                table: "PlanningSites");

            migrationBuilder.DropIndex(
                name: "IX_Compliances_AreaId",
                table: "Compliances");

            migrationBuilder.DropIndex(
                name: "IX_Compliances_PropertyId",
                table: "Compliances");

            migrationBuilder.DropIndex(
                name: "IX_ChemicalProductPropertieSites_PropertyId",
                table: "ChemicalProductPropertieSites");

            migrationBuilder.DropIndex(
                name: "IX_ChemicalProductProperties_PropertyId",
                table: "ChemicalProductProperties");

            migrationBuilder.DropIndex(
                name: "IX_AreaRulePlannings_AreaId",
                table: "AreaRulePlannings");

            migrationBuilder.DropIndex(
                name: "IX_AreaRulePlannings_PropertyId",
                table: "AreaRulePlannings");

            migrationBuilder.DropColumn(
                name: "ItemPlanningTagId",
                table: "AreaRulesPlanningVersions");

            migrationBuilder.DropColumn(
                name: "ItemPlanningTagId",
                table: "AreaRulePlannings");
        }
    }
}
