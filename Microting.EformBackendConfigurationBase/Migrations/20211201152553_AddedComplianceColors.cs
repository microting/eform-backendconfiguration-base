using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddedComplianceColors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PropertyComplianceColorId",
                table: "PropertieVersions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PropertyComplianceColorId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "PropertyComplianceColors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyComplianceColors", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "PropertyComplianceColors",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Success" });

            migrationBuilder.InsertData(
                table: "PropertyComplianceColors",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "Warning" });

            migrationBuilder.InsertData(
                table: "PropertyComplianceColors",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3, "Danger" });

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PropertyComplianceColorId",
                table: "Properties",
                column: "PropertyComplianceColorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_PropertyComplianceColors_PropertyComplianceColorId",
                table: "Properties",
                column: "PropertyComplianceColorId",
                principalTable: "PropertyComplianceColors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_PropertyComplianceColors_PropertyComplianceColorId",
                table: "Properties");

            migrationBuilder.DropTable(
                name: "PropertyComplianceColors");

            migrationBuilder.DropIndex(
                name: "IX_Properties_PropertyComplianceColorId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "PropertyComplianceColorId",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "PropertyComplianceColorId",
                table: "Properties");
        }
    }
}
