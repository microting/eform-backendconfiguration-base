using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingEntitySearchListChemicalRegNosToProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ChemicalLastUpdatedAt",
                table: "PropertieVersions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySearchListChemicalRegNos",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ChemicalLastUpdatedAt",
                table: "Properties",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySearchListChemicalRegNos",
                table: "Properties",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChemicalLastUpdatedAt",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "EntitySearchListChemicalRegNos",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "ChemicalLastUpdatedAt",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "EntitySearchListChemicalRegNos",
                table: "Properties");
        }
    }
}
