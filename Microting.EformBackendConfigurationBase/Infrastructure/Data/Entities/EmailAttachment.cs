using System.ComponentModel.DataAnnotations.Schema;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class EmailAttachment : PnBase
{
    [ForeignKey("Email")]
    public int EmailId { get; set; }
    public string ResourceName { get; set; }
    public string CidName { get; set; }
}