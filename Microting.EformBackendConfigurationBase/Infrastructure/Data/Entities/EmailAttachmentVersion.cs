using Microting.eFormApi.BasePn.Infrastructure.Database.Base;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class EmailAttachmentVersion : BaseEntity
{
    public int EmailAttachmentId { get; set; }
    public int EmailId { get; set; }
    public string ResourceName { get; set; }
    public string CidName { get; set; }
}