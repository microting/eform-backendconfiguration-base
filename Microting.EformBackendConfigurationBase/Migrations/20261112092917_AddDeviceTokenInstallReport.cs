using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceTokenInstallReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppBuild",
                table: "DeviceTokenVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AppVersion",
                table: "DeviceTokenVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DeviceManufacturer",
                table: "DeviceTokenVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DeviceModel",
                table: "DeviceTokenVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "DeviceTokenVersions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsVersion",
                table: "DeviceTokenVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AppBuild",
                table: "DeviceTokens",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AppVersion",
                table: "DeviceTokens",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DeviceManufacturer",
                table: "DeviceTokens",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DeviceModel",
                table: "DeviceTokens",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "DeviceTokens",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsVersion",
                table: "DeviceTokens",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppBuild",
                table: "DeviceTokenVersions");

            migrationBuilder.DropColumn(
                name: "AppVersion",
                table: "DeviceTokenVersions");

            migrationBuilder.DropColumn(
                name: "DeviceManufacturer",
                table: "DeviceTokenVersions");

            migrationBuilder.DropColumn(
                name: "DeviceModel",
                table: "DeviceTokenVersions");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "DeviceTokenVersions");

            migrationBuilder.DropColumn(
                name: "OsVersion",
                table: "DeviceTokenVersions");

            migrationBuilder.DropColumn(
                name: "AppBuild",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "AppVersion",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "DeviceManufacturer",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "DeviceModel",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "OsVersion",
                table: "DeviceTokens");
        }
    }
}
