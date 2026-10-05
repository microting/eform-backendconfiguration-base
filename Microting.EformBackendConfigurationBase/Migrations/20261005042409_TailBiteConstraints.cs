using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class TailBiteConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TailBiteRegistrationLocations_RegistrationId",
                table: "TailBiteRegistrationLocations");

            migrationBuilder.CreateIndex(
                name: "IX_TailBiteRegistrationLocations_RegistrationId_LocationId",
                table: "TailBiteRegistrationLocations",
                columns: new[] { "RegistrationId", "LocationId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TailBiteAssessmentActions_TailBiteRiskAssessments_Assessment~",
                table: "TailBiteAssessmentActions",
                column: "AssessmentId",
                principalTable: "TailBiteRiskAssessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TailBiteOutbreakLinks_TailBiteOutbreaks_OutbreakId",
                table: "TailBiteOutbreakLinks",
                column: "OutbreakId",
                principalTable: "TailBiteOutbreaks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TailBiteOutbreakLinks_TailBiteRegistrationLocations_Registra~",
                table: "TailBiteOutbreakLinks",
                column: "RegistrationLocationId",
                principalTable: "TailBiteRegistrationLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TailBiteRegistrationActions_TailBiteRegistrations_Registrati~",
                table: "TailBiteRegistrationActions",
                column: "RegistrationId",
                principalTable: "TailBiteRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TailBiteRegistrationLocations_TailBiteRegistrations_Registra~",
                table: "TailBiteRegistrationLocations",
                column: "RegistrationId",
                principalTable: "TailBiteRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TailBiteRiskAssessments_TailBiteOutbreaks_OutbreakId",
                table: "TailBiteRiskAssessments",
                column: "OutbreakId",
                principalTable: "TailBiteOutbreaks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TailBiteAssessmentActions_TailBiteRiskAssessments_Assessment~",
                table: "TailBiteAssessmentActions");

            migrationBuilder.DropForeignKey(
                name: "FK_TailBiteOutbreakLinks_TailBiteOutbreaks_OutbreakId",
                table: "TailBiteOutbreakLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_TailBiteOutbreakLinks_TailBiteRegistrationLocations_Registra~",
                table: "TailBiteOutbreakLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_TailBiteRegistrationActions_TailBiteRegistrations_Registrati~",
                table: "TailBiteRegistrationActions");

            migrationBuilder.DropForeignKey(
                name: "FK_TailBiteRegistrationLocations_TailBiteRegistrations_Registra~",
                table: "TailBiteRegistrationLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_TailBiteRiskAssessments_TailBiteOutbreaks_OutbreakId",
                table: "TailBiteRiskAssessments");

            migrationBuilder.DropIndex(
                name: "IX_TailBiteRegistrationLocations_RegistrationId_LocationId",
                table: "TailBiteRegistrationLocations");

            migrationBuilder.CreateIndex(
                name: "IX_TailBiteRegistrationLocations_RegistrationId",
                table: "TailBiteRegistrationLocations",
                column: "RegistrationId");
        }
    }
}
