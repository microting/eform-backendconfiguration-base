using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations;

/// <inheritdoc />
public partial class AddTaskChangeFeed : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Additive only: one new table, no existing table or column is touched.
        //
        // The SQL is what EF generates for the model (CreateTable + CreateIndex), folded into one
        // guarded statement. MariaDB DDL is not transactional (see DeviceTokenIdentityModel and
        // AddArchiveFolders): a pod that dies between EF's two statements would leave the table
        // without its index and fail the re-run on "table exists". One CREATE TABLE IF NOT EXISTS
        // carrying the index either exists in full or not at all, and a re-run is a no-op.
        migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `TaskChangeFeedEntries` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `CreatedAt` datetime(6) NOT NULL,
    `PropertyId` int NULL,
    `SdkSiteId` int NULL,
    `AreaRulePlanningId` int NULL,
    `PlanDayKey` varchar(10) CHARACTER SET utf8mb4 NULL,
    `ComplianceId` int NULL,
    `Kind` varchar(16) CHARACTER SET utf8mb4 NOT NULL,
    `Source` varchar(64) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_TaskChangeFeedEntries` PRIMARY KEY (`Id`),
    INDEX `IX_TaskChangeFeedEntries_CreatedAt` (`CreatedAt`)
) CHARACTER SET=utf8mb4;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS `TaskChangeFeedEntries`;");
    }
}
