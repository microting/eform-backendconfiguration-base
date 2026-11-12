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
            // MariaDB DDL is not transactional: a pod that dies after some of these
            // statements but before EF records the migration re-runs Up() on the next
            // start. Guarded like DeviceTokenIdentityModel so that re-run finishes the
            // job instead of failing on a duplicate column.
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` ADD COLUMN IF NOT EXISTS `AppVersion` varchar(32) CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` ADD COLUMN IF NOT EXISTS `AppBuild` varchar(32) CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` ADD COLUMN IF NOT EXISTS `DeviceModel` varchar(128) CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` ADD COLUMN IF NOT EXISTS `DeviceManufacturer` varchar(128) CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` ADD COLUMN IF NOT EXISTS `OsVersion` varchar(64) CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` ADD COLUMN IF NOT EXISTS `LastSeenAt` datetime(6) NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokenVersions` ADD COLUMN IF NOT EXISTS `AppVersion` longtext CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokenVersions` ADD COLUMN IF NOT EXISTS `AppBuild` longtext CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokenVersions` ADD COLUMN IF NOT EXISTS `DeviceModel` longtext CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokenVersions` ADD COLUMN IF NOT EXISTS `DeviceManufacturer` longtext CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokenVersions` ADD COLUMN IF NOT EXISTS `OsVersion` longtext CHARACTER SET utf8mb4 NULL;");
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokenVersions` ADD COLUMN IF NOT EXISTS `LastSeenAt` datetime(6) NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE `DeviceTokens` DROP COLUMN IF EXISTS `AppVersion`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokens` DROP COLUMN IF EXISTS `AppBuild`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokens` DROP COLUMN IF EXISTS `DeviceModel`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokens` DROP COLUMN IF EXISTS `DeviceManufacturer`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokens` DROP COLUMN IF EXISTS `OsVersion`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokens` DROP COLUMN IF EXISTS `LastSeenAt`;");

            migrationBuilder.Sql("ALTER TABLE `DeviceTokenVersions` DROP COLUMN IF EXISTS `AppVersion`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokenVersions` DROP COLUMN IF EXISTS `AppBuild`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokenVersions` DROP COLUMN IF EXISTS `DeviceModel`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokenVersions` DROP COLUMN IF EXISTS `DeviceManufacturer`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokenVersions` DROP COLUMN IF EXISTS `OsVersion`;");
            migrationBuilder.Sql("ALTER TABLE `DeviceTokenVersions` DROP COLUMN IF EXISTS `LastSeenAt`;");
        }
    }
}
