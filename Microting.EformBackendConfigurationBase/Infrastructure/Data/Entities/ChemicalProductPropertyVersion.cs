using System;
using Microting.eFormApi.BasePn.Infrastructure.Database.Base;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class ChemicalProductPropertyVersion :BaseEntity
{
    public int ChemicalProductPropertyId { get; set; }
    public int ChemicalId { get; set; }
    public int ProductId { get; set; }
    public int PropertyId { get; set; }
    public int SdkCaseId { get; set; }
    public string Locations { get; set; }
    public int LanguageId { get; set; }
    public int SdkSiteId { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string LastFolderName { get; set; }
}