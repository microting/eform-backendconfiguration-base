/*
The MIT License (MIT)

Copyright (c) 2007 - 2022 Microting A/S

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data
{
    using eFormApi.BasePn.Abstractions;
    using eFormApi.BasePn.Infrastructure.Database.Entities;
    using Entities;
    using Microsoft.EntityFrameworkCore;
    using Extensions.Seed;

    public class BackendConfigurationPnDbContext: DbContext, IPluginDbContext
    {
        public BackendConfigurationPnDbContext() { }

        public BackendConfigurationPnDbContext(DbContextOptions<BackendConfigurationPnDbContext> options) : base(options)
        {
        }

        public DbSet<PropertyWorker> PropertyWorkers { get; set; }
        public DbSet<PropertyWorkerVersion> PropertyWorkerVersions { get; set; }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyVersion> PropertieVersions { get; set; }

        public DbSet<Area> Areas { get; set; }
        public DbSet<AreaVersion> AreaVersions { get; set; }

        public DbSet<AreaTranslation> AreaTranslations { get; set; }
        public DbSet<AreaTranslationVersion> AreaTranslationVersions { get; set; }

        public DbSet<AreaRule> AreaRules { get; set; }
        public DbSet<AreaRuleVersion> AreaRuleVersions { get; set; }

        public DbSet<AreaProperty> AreaProperties { get; set; }
        public DbSet<AreaPropertyVersion> AreaPropertyVersions { get; set; }

        public DbSet<AreaRuleTranslation> AreaRuleTranslations { get; set; }
        public DbSet<AreaRuleTranslationVersion> AreaRuleTranslationVersions { get; set; }

        public DbSet<PropertySelectedLanguage> PropertySelectedLanguages { get; set; }
        public DbSet<PropertySelectedLanguageVersion> PropertySelectedLanguageVersions { get; set; }

        public DbSet<AreaRulePlanning> AreaRulePlannings { get; set; }
        public DbSet<AreaRulePlanningVersion> AreaRulesPlanningVersions { get; set; }

        public DbSet<PlanningSite> PlanningSites { get; set; }
        public DbSet<PlanningSiteVersion> PlanningSitesVersions { get; set; }

        public DbSet<AreaRuleInitialField> AreaRuleInitialFields { get; set; }

        public DbSet<AreaInitialField> AreaInitialFields { get; set; }
        public DbSet<AreaInitialFieldVersion> AreaInitialFieldVersions { get; set; }

        public DbSet<ProperyAreaFolder> ProperyAreaFolders { get; set; }
        public DbSet<ProperyAreaFolderVersion> ProperyAreaFolderVersions { get; set; }

        public DbSet<Compliance> Compliances { get; set; }
        public DbSet<ComplianceVersion> ComplianceVersions { get; set; }

        public DbSet<WorkorderCase> WorkorderCases { get; set; }
        public DbSet<WorkorderCaseVersion> WorkorderCaseVersions { get; set; }

        public DbSet<WorkorderCaseImage> WorkorderCaseImages { get; set; }
        public DbSet<WorkorderCaseImageVersion> WorkorderCaseImageVersions { get; set; }

        public DbSet<ChemicalProductPropertySite> ChemicalProductPropertieSites { get; set; }
        public DbSet<ChemicalProductPropertySiteVersion> ChemicalProductPropertyVersionSites { get; set; }

        public DbSet<PoolHour> PoolHours { get; set; }
        public DbSet<PoolHourVersion> PoolHourVersions { get; set; }

        public DbSet<PoolAccident> PoolAccidents { get; set; }
        public DbSet<PoolAccidentVersion> PoolAccidentVersions { get; set; }

        public DbSet<PoolHourResult> PoolHourResults { get; set; }
        public DbSet<PoolHourResultVersion> PoolHourResultVersions { get; set; }

        public DbSet<PoolHistorySite> PoolHistorySites { get; set; }
        public DbSet<PoolHistorySiteVersion> PoolHistorySiteVersions { get; set; }

        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailVersion> EmailVersions { get; set; }

        public DbSet<EmailAttachment> EmailAttachments { get; set; }
        public DbSet<EmailAttachmentVersion> EmailAttachmentVersions { get; set; }

        public DbSet<File> Files { get; set; }
        public DbSet<FileVersion> FileVersions { get; set; }

        public DbSet<FileTag> FileTags { get; set; }
        public DbSet<FileTagVersion> FileTagVersions { get; set; }

        public DbSet<FileTags> FilesTags { get; set; }
        public DbSet<FileTagsVersion> FilesTagsVersions { get; set; }

        public DbSet<UploadedData> UploadedDatas { get; set; }
        public DbSet<UploadedDataVersion> UploadedDataVersions { get; set; }

        public DbSet<PropertyFile> PropertyFiles { get; set; }
        public DbSet<PropertyFileVersion> PropertyFileVersions { get; set; }

        public DbSet<TaskTrackerColumn> TaskTrackerColumns { get; set; }
        public DbSet<TaskTrackerColumnVersion> TaskTrackerColumnVersions { get; set; }

        public DbSet<AreaRulePlanningTag> AreaRulePlanningTags { get; set; }
        public DbSet<AreaRulePlanningTagVersion> AreaRulePlanningTagVersion { get; set; }

        // common tables
        public DbSet<PluginConfigurationValue> PluginConfigurationValues { get; set; }
        public DbSet<PluginConfigurationValueVersion> PluginConfigurationValueVersions { get; set; }
        public DbSet<PluginPermission> PluginPermissions { get; set; }
        public DbSet<PluginGroupPermission> PluginGroupPermissions { get; set; }
        public DbSet<PluginGroupPermissionVersion> PluginGroupPermissionVersions { get; set; }
        public DbSet<ChemicalProductProperty> ChemicalProductProperties { get; set; }
        public DbSet<ChemicalProductPropertyVersion> ChemicalProductPropertyVersions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Compliance>()
                .HasIndex(e => new { e.PlanningId, e.Deadline })
                .IsUnique();

            modelBuilder.Entity<AreaRuleTranslation>().HasOne(x => x.AreaRule)
                .WithMany(x => x.AreaRuleTranslations)
                .HasForeignKey(x => x.AreaRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropertySelectedLanguage>().HasOne(x => x.Property)
                .WithMany(x => x.SelectedLanguages)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProperyAreaFolder>().HasOne(x => x.AreaProperty)
                .WithMany(x => x.ProperyAreaFolders)
                .HasForeignKey(x => x.ProperyAreaAsignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AreaRulePlanning>().HasOne(x => x.AreaRule)
                .WithMany(x => x.AreaRulesPlannings)
                .HasForeignKey(x => x.AreaRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AreaRule>().HasOne(x => x.Area)
                .WithMany(x => x.AreaRules)
                .HasForeignKey(x => x.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlanningSite>().HasOne(x => x.AreaRulePlanning)
                .WithMany(x => x.PlanningSites)
                .HasForeignKey(x => x.AreaRulePlanningsId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropertyWorker>().HasOne(x => x.Property)
                .WithMany(x => x.PropertyWorkers)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AreaProperty>().HasOne(x => x.Area)
                .WithMany(x => x.AreaProperties)
                .HasForeignKey(x => x.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AreaProperty>().HasOne(x => x.Property)
                .WithMany(x => x.AreaProperties)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkorderCase>().HasOne(x => x.PropertyWorker)
                .WithMany(x => x.WorkorderCases)
                .HasForeignKey(x => x.PropertyWorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AreaRulePlanningTag>().HasOne(x => x.AreaRulePlanning)
                .WithMany(x => x.AreaRulePlanningTags)
                .HasForeignKey(x => x.AreaRulePlanningId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.SeedLatest();
        }
    }
}