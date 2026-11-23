using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations;

/// <inheritdoc />
public partial class AddTaskNotifications : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Additive only: one new table, a nullable TimeZone on Properties/PropertieVersions and a
        // TaskReminders flag (default false) on DeviceTokens/DeviceTokenVersions.
        //
        // The SQL is what EF generates for the model, guarded: MariaDB DDL is not transactional (see
        // DeviceTokenIdentityModel and AddTaskChangeFeed), so a pod that dies part-way re-runs Up
        // from the top, and every statement must then be a no-op. The table carries its indexes in
        // the one CREATE TABLE, so it exists in full or not at all.
        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `TaskNotifications` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `SdkSiteId` int NOT NULL,
    `PropertyId` int NOT NULL,
    `AreaRulePlanningId` int NOT NULL,
    `PlanDayKey` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
    `ComplianceId` int NULL,
    `Kind` varchar(16) CHARACTER SET utf8mb4 NOT NULL,
    `PlannedStartUtc` datetime(6) NOT NULL,
    `SendAtUtc` datetime(6) NOT NULL,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Body` varchar(400) CHARACTER SET utf8mb4 NOT NULL,
    `State` tinyint unsigned NOT NULL,
    `LeaseUntilUtc` datetime(6) NULL,
    `Attempts` int NOT NULL,
    `NextAttemptAtUtc` datetime(6) NOT NULL,
    `SentAtUtc` datetime(6) NULL,
    `WithdrawnAtUtc` datetime(6) NULL,
    `LastError` varchar(64) CHARACTER SET utf8mb4 NULL,
    `CreatedAtUtc` datetime(6) NOT NULL,
    `UpdatedAtUtc` datetime(6) NOT NULL,
    CONSTRAINT `PK_TaskNotifications` PRIMARY KEY (`Id`),
    UNIQUE INDEX `UX_TaskNotifications_Site_Task_Day_Start_Kind` (`SdkSiteId`, `AreaRulePlanningId`, `PlanDayKey`, `PlannedStartUtc`, `Kind`),
    INDEX `IX_TaskNotifications_State_NextAttemptAtUtc` (`State`, `NextAttemptAtUtc`),
    INDEX `IX_TaskNotifications_PropertyId_PlanDayKey` (`PropertyId`, `PlanDayKey`),
    INDEX `IX_TaskNotifications_CreatedAtUtc` (`CreatedAtUtc`)
) CHARACTER SET=utf8mb4;");

        migrationBuilder.Sql(
            "ALTER TABLE `Properties` ADD COLUMN IF NOT EXISTS `TimeZone` varchar(64) CHARACTER SET utf8mb4 NULL;");
        migrationBuilder.Sql(
            "ALTER TABLE `PropertieVersions` ADD COLUMN IF NOT EXISTS `TimeZone` longtext CHARACTER SET utf8mb4 NULL;");

        // Opt-in for task reminders: existing installs and old app versions stay false.
        migrationBuilder.Sql(
            "ALTER TABLE `DeviceTokens` ADD COLUMN IF NOT EXISTS `TaskReminders` tinyint(1) NOT NULL DEFAULT FALSE;");
        migrationBuilder.Sql(
            "ALTER TABLE `DeviceTokenVersions` ADD COLUMN IF NOT EXISTS `TaskReminders` tinyint(1) NOT NULL DEFAULT FALSE;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE `DeviceTokenVersions` DROP COLUMN IF EXISTS `TaskReminders`;");
        migrationBuilder.Sql("ALTER TABLE `DeviceTokens` DROP COLUMN IF EXISTS `TaskReminders`;");
        migrationBuilder.Sql("ALTER TABLE `PropertieVersions` DROP COLUMN IF EXISTS `TimeZone`;");
        migrationBuilder.Sql("ALTER TABLE `Properties` DROP COLUMN IF EXISTS `TimeZone`;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS `TaskNotifications`;");
    }
}
