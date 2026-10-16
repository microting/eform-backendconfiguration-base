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
            // -> batched move -> index -> final sweep -> FK.
            //
            // The steps are ordered by hand. EF's own order would add the FK on Files.FolderId
            // straight away, and that cannot work on a table whose rows have no folder yet.
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

            // 2. New columns. FolderId is added nullable and stays nullable (see the note before the FK).
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
            //    linear, because there is no index on FolderId yet. `OR FolderId = 0` also moves a
            //    stray 0, which the FK would reject. suppressTransaction: each batch commits on its own.
            migrationBuilder.Sql(MoveIntoDefaultFolder("Files"), suppressTransaction: true);
            migrationBuilder.Sql(MoveIntoDefaultFolder("FileVersions"), suppressTransaction: true);

            // 5. Index first, so that the final sweep below and the FK can use it.
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS `IX_Files_FolderId` ON `Files` (`FolderId`);");

            // 6. Final sweep. It catches a row that an old pod inserted after step 4 read
            //    MAX(Id). Rows inserted after the migration by code that does not know folders
            //    keep FolderId NULL, which means the default folder. If Up crashes anywhere, the
            //    re-run from the top catches what is left, as in DeviceTokenIdentityModel.
            migrationBuilder.Sql(@"
UPDATE `Files`
SET `FolderId` = (SELECT MIN(`Id`) FROM `ArchiveFolders` WHERE `IsDefault` = 1)
WHERE `FolderId` IS NULL OR `FolderId` = 0;");

            // FileVersions.FolderId is swept as well, so that history written during the deploy
            // also lands in Arkiv.
            migrationBuilder.Sql(@"
UPDATE `FileVersions`
SET `FolderId` = (SELECT MIN(`Id`) FROM `ArchiveFolders` WHERE `IsDefault` = 1)
WHERE `FolderId` IS NULL OR `FolderId` = 0;");

            // Files.FolderId deliberately stays nullable: no NOT NULL, no DEFAULT, no triggers.
            // After this migration a host can still run plugin code that does not know folders
            // (the service plugin also runs Migrate()), and its inserts leave FolderId NULL.
            // NULL means the default folder; the API resolves it by IsDefault, never by a fixed
            // id. A later migration fills the NULLs and makes the column NOT NULL.

            // 7. FK. Files.FolderId only: FileVersions.FolderId deliberately gets no FK.
            migrationBuilder.Sql(@"
ALTER TABLE `Files`
    ADD CONSTRAINT `FK_Files_ArchiveFolders_FolderId`
    FOREIGN KEY IF NOT EXISTS (`FolderId`) REFERENCES `ArchiveFolders` (`Id`) ON DELETE RESTRICT;");
        }

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
