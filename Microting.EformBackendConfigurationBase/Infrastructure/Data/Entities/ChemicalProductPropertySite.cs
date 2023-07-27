namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class ChemicalProductPropertySite : PnBase
{
    public int ChemicalId { get; set; }
    public int ProductId { get; set; }
    public int PropertyId { get; set; }
    public virtual Property Property { get; set; }
    public int SdkCaseId { get; set; }
    public int SdkSiteId { get; set; }
    public int LanguageId { get; set; }
}