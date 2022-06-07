namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class ChemicalProductProperty : PnBase
{
    public int ChemicalId { get; set; }
    public int ProductId { get; set; }
    public int PropertyId { get; set; }
}