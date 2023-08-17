using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class DeleteNotNeededLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkorderCaseImages_UploadedDataId",
                table: "WorkorderCaseImages",
                column: "UploadedDataId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkorderCaseImages_WorkorderCaseId",
                table: "WorkorderCaseImages",
                column: "WorkorderCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PoolHours_AreaRuleId",
                table: "PoolHours",
                column: "AreaRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PoolHistorySites_AreaRuleId",
                table: "PoolHistorySites",
                column: "AreaRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PoolAccidents_AreaRuleId",
                table: "PoolAccidents",
                column: "AreaRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningSites_AreaId",
                table: "PlanningSites",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningSites_AreaRuleId",
                table: "PlanningSites",
                column: "AreaRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Compliances_AreaId",
                table: "Compliances",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Compliances_PropertyId",
                table: "Compliances",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalProductPropertieSites_PropertyId",
                table: "ChemicalProductPropertieSites",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalProductProperties_PropertyId",
                table: "ChemicalProductProperties",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaRulePlannings_AreaId",
                table: "AreaRulePlannings",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaRulePlannings_PropertyId",
                table: "AreaRulePlannings",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AreaRulePlannings_Areas_AreaId",
                table: "AreaRulePlannings",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AreaRulePlannings_Properties_PropertyId",
                table: "AreaRulePlannings",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChemicalProductProperties_Properties_PropertyId",
                table: "ChemicalProductProperties",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChemicalProductPropertieSites_Properties_PropertyId",
                table: "ChemicalProductPropertieSites",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Compliances_Areas_AreaId",
                table: "Compliances",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Compliances_Properties_PropertyId",
                table: "Compliances",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningSites_AreaRules_AreaRuleId",
                table: "PlanningSites",
                column: "AreaRuleId",
                principalTable: "AreaRules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningSites_Areas_AreaId",
                table: "PlanningSites",
                column: "AreaId",
                principalTable: "Areas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PoolAccidents_AreaRules_AreaRuleId",
                table: "PoolAccidents",
                column: "AreaRuleId",
                principalTable: "AreaRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoolHistorySites_AreaRules_AreaRuleId",
                table: "PoolHistorySites",
                column: "AreaRuleId",
                principalTable: "AreaRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoolHours_AreaRules_AreaRuleId",
                table: "PoolHours",
                column: "AreaRuleId",
                principalTable: "AreaRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkorderCaseImages_UploadedDatas_UploadedDataId",
                table: "WorkorderCaseImages",
                column: "UploadedDataId",
                principalTable: "UploadedDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkorderCaseImages_WorkorderCases_WorkorderCaseId",
                table: "WorkorderCaseImages",
                column: "WorkorderCaseId",
                principalTable: "WorkorderCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
