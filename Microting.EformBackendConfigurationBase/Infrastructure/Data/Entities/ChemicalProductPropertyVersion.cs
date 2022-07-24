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
}