using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveFolders : Migration
    {
        // Batches of 5,000 ids. Each UPDATE commits on its own (autocommit, and the step runs
        // with suppressTransaction), so a large tenant never holds one huge lock.
        private const int MoveBatchSize = 5000;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step roadmap: create tables -> add nullable/defaulted columns -> default folder
            // -> batched move -> index -> default-folder trigger -> final sweep -> tighten -> FK.
            //
            // The steps are ordered by hand. EF's own order would add Files.FolderId as NOT NULL
            // DEFAULT 0 with its FK straight away, and that cannot work on a table that already
            // has rows.
            //
            // Each step is written so that running Up again from the top does nothing for the
            // steps that are already done (same reasoning as DeviceTokenIdentityModel). MariaDB
            // commits each DDL statement on its own, so this migration is NOT atomic.
            // __EFMigrationsHistory is written only after the last statement, and a pod that
            // dies partway re-runs Up from the top at its next start. The guards use MariaDB's
            // IF [NOT] EXISTS clauses and BEGIN NOT ATOMIC blocks with local variables. They do
            // not use @user variables, because MySqlConnector rejects those without
            // AllowUserVariables=True.

            // 1. New tables. Each CREATE TABLE carries its PK, FKs and indexes, so the table
            //    either exists in full or does not exist at all.
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `ArchiveFolders` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `ParentId` int NULL,
    `IsDefault` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `WorkflowState` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedByUserId` int NOT NULL,
    `UpdatedByUserId` int NOT NULL,
    `Version` int NOT NULL,
    CONSTRAINT `PK_ArchiveFolders` PRIMARY KEY (`Id`),
    INDEX `IX_ArchiveFolders_ParentId` (`ParentId`),
    CONSTRAINT `FK_ArchiveFolders_ArchiveFolders_ParentId` FOREIGN KEY (`ParentId`)
        REFERENCES `ArchiveFolders` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `ArchiveFolderVersions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ArchiveFolderId` int NOT NULL,
    `Name` longtext CHARACTER SET utf8mb4 NULL,
    `ParentId` int NULL,
    `IsDefault` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `WorkflowState` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedByUserId` int NOT NULL,
    `UpdatedByUserId` int NOT NULL,
    `Version` int NOT NULL,
    CONSTRAINT `PK_ArchiveFolderVersions` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `ArchiveFolderReadTags` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ArchiveFolderId` int NOT NULL,
    `SdkTagId` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `WorkflowState` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedByUserId` int NOT NULL,
    `UpdatedByUserId` int NOT NULL,
    `Version` int NOT NULL,
    CONSTRAINT `PK_ArchiveFolderReadTags` PRIMARY KEY (`Id`),
    INDEX `IX_ArchiveFolderReadTags_ArchiveFolderId` (`ArchiveFolderId`),
    INDEX `IX_ArchiveFolderReadTags_SdkTagId` (`SdkTagId`),
    CONSTRAINT `FK_ArchiveFolderReadTags_ArchiveFolders_ArchiveFolderId` FOREIGN KEY (`ArchiveFolderId`)
        REFERENCES `ArchiveFolders` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `ArchiveFolderReadTagVersions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ArchiveFolderReadTagId` int NOT NULL,
    `ArchiveFolderId` int NOT NULL,
    `SdkTagId` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `WorkflowState` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedByUserId` int NOT NULL,
    `UpdatedByUserId` int NOT NULL,
    `Version` int NOT NULL,
    CONSTRAINT `PK_ArchiveFolderReadTagVersions` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `FileReadTags` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FileId` int NOT NULL,
    `SdkTagId` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `WorkflowState` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedByUserId` int NOT NULL,
    `UpdatedByUserId` int NOT NULL,
    `Version` int NOT NULL,
    CONSTRAINT `PK_FileReadTags` PRIMARY KEY (`Id`),
    INDEX `IX_FileReadTags_FileId` (`FileId`),
    INDEX `IX_FileReadTags_SdkTagId` (`SdkTagId`),
    CONSTRAINT `FK_FileReadTags_Files_FileId` FOREIGN KEY (`FileId`)
        REFERENCES `Files` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `FileReadTagVersions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FileReadTagId` int NOT NULL,
    `FileId` int NOT NULL,
    `SdkTagId` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `WorkflowState` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedByUserId` int NOT NULL,
    `UpdatedByUserId` int NOT NULL,
    `Version` int NOT NULL,
    CONSTRAINT `PK_FileReadTagVersions` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;");

            // 2. New columns. FolderId stays nullable until the data step is done.
            //    DocumentDateState and OwnAccess are added NOT NULL DEFAULT 0, so existing files
            //    need no UPDATE for them, none of them lands in "Dato skal tjekkes", and an old pod
            //    that inserts without these columns still works. The defaults are kept for good
            //    (they match the CLR defaults None/false).
            migrationBuilder.Sql(@"
ALTER TABLE `Files`
    ADD COLUMN IF NOT EXISTS `FolderId` int NULL,
    ADD COLUMN IF NOT EXISTS `DocumentDate` date NULL,
    ADD COLUMN IF NOT EXISTS `DocumentDateState` int NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS `OwnAccess` tinyint(1) NOT NULL DEFAULT 0;");

            migrationBuilder.Sql(@"
ALTER TABLE `FileVersions`
    ADD COLUMN IF NOT EXISTS `FolderId` int NULL,
    ADD COLUMN IF NOT EXISTS `DocumentDate` date NULL,
    ADD COLUMN IF NOT EXISTS `DocumentDateState` int NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS `OwnAccess` tinyint(1) NOT NULL DEFAULT 0;");

            migrationBuilder.Sql(
                "ALTER TABLE `InboxSuggestions` ADD COLUMN IF NOT EXISTS `TargetDate` date NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE `InboxSuggestionVersions` ADD COLUMN IF NOT EXISTS `TargetDate` date NULL;");

            // 3. The default folder "Arkiv", with every PnBase column filled the way PnBase.Create
            //    fills them. It is created only when there is no default yet, so a re-run never
            //    makes a second one. Its version row is created only when it is missing, so a crash
            //    between the two INSERTs heals on the next run.
            migrationBuilder.Sql(@"
INSERT INTO `ArchiveFolders`
    (`Name`, `ParentId`, `IsDefault`, `CreatedAt`, `UpdatedAt`, `WorkflowState`,
     `CreatedByUserId`, `UpdatedByUserId`, `Version`)
SELECT 'Arkiv', NULL, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), 'created', 0, 0, 1
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `ArchiveFolders` WHERE `IsDefault` = 1);");

            migrationBuilder.Sql(@"
INSERT INTO `ArchiveFolderVersions`
    (`ArchiveFolderId`, `Name`, `ParentId`, `IsDefault`, `CreatedAt`, `UpdatedAt`, `WorkflowState`,
     `CreatedByUserId`, `UpdatedByUserId`, `Version`)
SELECT f.`Id`, f.`Name`, f.`ParentId`, f.`IsDefault`, f.`CreatedAt`, f.`UpdatedAt`, f.`WorkflowState`,
       f.`CreatedByUserId`, f.`UpdatedByUserId`, f.`Version`
FROM `ArchiveFolders` f
WHERE f.`IsDefault` = 1
  AND NOT EXISTS (SELECT 1 FROM `ArchiveFolderVersions` v WHERE v.`ArchiveFolderId` = f.`Id`);");

            // 4. Move every file and every file version (all workflow states) into the default
            //    folder, walking the primary key in ranges of MoveBatchSize. The ranges keep the cost
            //    linear, because there is no index on FolderId yet. `OR FolderId = 0` covers a
            //    server that is not in strict mode, where a failed earlier tightening would have
            //    turned NULL into 0. suppressTransaction: each batch commits on its own.
            migrationBuilder.Sql(MoveIntoDefaultFolder("Files"), suppressTransaction: true);
            migrationBuilder.Sql(MoveIntoDefaultFolder("FileVersions"), suppressTransaction: true);

            // 5. Index first, so that the final sweep below and the FK can use it.
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS `IX_Files_FolderId` ON `Files` (`FolderId`);");

            // 6. Rollout safety net: DEFAULT 0 on Files.FolderId (step 8) and two triggers,
            //    TR_Files_DefaultFolder and TR_FileVersions_DefaultFolder.
            //
            //    Why they exist: the service plugin runs Migrate() at startup and can pick up this
            //    base before the new API plugin ships. Old plugin code inserts Files without
            //    FolderId, or with 0, and would fail with ERROR 1364 (no default) or 1452 (FK).
            //    The new plugin always sends FolderId, so the EF model has no default: EF never
            //    compares the DB default. The default and both triggers can be removed together
            //    in a later migration once all hosts run the new plugin.
            //
            //    What the triggers do: a row with FolderId NULL or 0 goes into the default
            //    folder. A non-zero unknown id is left alone, so the FK on Files still rejects it
            //    with 1452. The FileVersions trigger is needed because an EF save with FolderId 0
            //    keeps 0 on the tracked entity (the Files trigger's value is not read back), and
            //    PnBase.Create copies that 0 into the version row. CREATE OR REPLACE makes a
            //    re-run replace them, never duplicate them. The single-statement body
            //    (IF ... END IF) is sent as one command, like the BEGIN NOT ATOMIC blocks in
            //    step 4, so no DELIMITER is needed.
            //
            //    Why they sit before the final sweep: from here on no new row can end up without
            //    a folder. Rows inserted earlier are caught by the sweep, later ones by a trigger.
            migrationBuilder.Sql(DefaultFolderTrigger("TR_Files_DefaultFolder", "Files"),
                suppressTransaction: true);
            migrationBuilder.Sql(DefaultFolderTrigger("TR_FileVersions_DefaultFolder", "FileVersions"),
                suppressTransaction: true);

            // 7. Final sweep. A row that an old pod inserted after step 4 read MAX(Id), but before
            //    the trigger existed, still has FolderId NULL. This statement catches it. If Up
            //    crashes anywhere, the re-run from the top catches what is left, as in
            //    DeviceTokenIdentityModel.
            migrationBuilder.Sql(@"
UPDATE `Files`
SET `FolderId` = (SELECT MIN(`Id`) FROM `ArchiveFolders` WHERE `IsDefault` = 1)
WHERE `FolderId` IS NULL OR `FolderId` = 0;");

            // FileVersions.FolderId stays nullable, so a late version row cannot fail the MODIFY.
            // It is swept anyway, so that history written during the deploy also lands in Arkiv.
            migrationBuilder.Sql(@"
UPDATE `FileVersions`
SET `FolderId` = (SELECT MIN(`Id`) FROM `ArchiveFolders` WHERE `IsDefault` = 1)
WHERE `FolderId` IS NULL OR `FolderId` = 0;");

            // 8. Tighten. NOT NULL, plus DEFAULT 0 for the safety net in step 6: an insert that
            //    omits the column is accepted by strict mode, and the trigger fixes it. MODIFY
            //    restates the whole column definition, so a re-run changes nothing.
            migrationBuilder.Sql(
                "ALTER TABLE `Files` MODIFY COLUMN `FolderId` int NOT NULL DEFAULT 0;");

            // 9. FK. Files.FolderId only: FileVersions.FolderId deliberately gets no FK.
            migrationBuilder.Sql(@"
ALTER TABLE `Files`
    ADD CONSTRAINT `FK_Files_ArchiveFolders_FolderId`
    FOREIGN KEY IF NOT EXISTS (`FolderId`) REFERENCES `ArchiveFolders` (`Id`) ON DELETE RESTRICT;");
        }

        private static string DefaultFolderTrigger(string name, string table) => $@"
CREATE OR REPLACE TRIGGER `{name}` BEFORE INSERT ON `{table}` FOR EACH ROW
IF NEW.`FolderId` IS NULL OR NEW.`FolderId` = 0 THEN
    SET NEW.`FolderId` = (SELECT `Id` FROM `ArchiveFolders`
                          WHERE `IsDefault` = 1 AND `WorkflowState` <> 'removed'
                          ORDER BY `Id` LIMIT 1);
END IF";

        // v_from and v_max are BIGINT, so that v_from + MoveBatchSize cannot overflow INT on the
        // last batch when ids come near 2,147,483,647.
        private static string MoveIntoDefaultFolder(string table) => $@"
BEGIN NOT ATOMIC
    DECLARE v_default_folder_id INT;
    DECLARE v_from BIGINT DEFAULT 0;
    DECLARE v_max BIGINT;
    SELECT MIN(`Id`) INTO v_default_folder_id FROM `ArchiveFolders` WHERE `IsDefault` = 1;
    IF v_default_folder_id IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'AddArchiveFolders: no default archive folder exists.';
    END IF;
    SELECT COALESCE(MAX(`Id`), 0) INTO v_max FROM `{table}`;
    WHILE v_from < v_max DO
        UPDATE `{table}` SET `FolderId` = v_default_folder_id
        WHERE `Id` > v_from AND `Id` <= v_from + {MoveBatchSize}
          AND (`FolderId` IS NULL OR `FolderId` = 0);
        SET v_from = v_from + {MoveBatchSize};
    END WHILE;
END";

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lossy: folder placement, read access, document dates and date suggestions are dropped.
            // Unlike Up, Down is NOT re-runnable, on purpose (plain EF calls, as in
            // DeviceTokenIdentityModel). Rollbacks are attended and done once.
            // The triggers read ArchiveFolders, so they go first.
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS `TR_Files_DefaultFolder`;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS `TR_FileVersions_DefaultFolder`;");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_ArchiveFolders_FolderId",
                table: "Files");

            migrationBuilder.DropTable(name: "ArchiveFolderReadTags");
            migrationBuilder.DropTable(name: "ArchiveFolderReadTagVersions");
            migrationBuilder.DropTable(name: "ArchiveFolderVersions");
            migrationBuilder.DropTable(name: "FileReadTags");
            migrationBuilder.DropTable(name: "FileReadTagVersions");
            migrationBuilder.DropTable(name: "ArchiveFolders");

            migrationBuilder.DropIndex(name: "IX_Files_FolderId", table: "Files");

            migrationBuilder.DropColumn(name: "FolderId", table: "Files");
            migrationBuilder.DropColumn(name: "DocumentDate", table: "Files");
            migrationBuilder.DropColumn(name: "DocumentDateState", table: "Files");
            migrationBuilder.DropColumn(name: "OwnAccess", table: "Files");

            migrationBuilder.DropColumn(name: "FolderId", table: "FileVersions");
            migrationBuilder.DropColumn(name: "DocumentDate", table: "FileVersions");
            migrationBuilder.DropColumn(name: "DocumentDateState", table: "FileVersions");
            migrationBuilder.DropColumn(name: "OwnAccess", table: "FileVersions");

            migrationBuilder.DropColumn(name: "TargetDate", table: "InboxSuggestions");
            migrationBuilder.DropColumn(name: "TargetDate", table: "InboxSuggestionVersions");
        }
    }
}
