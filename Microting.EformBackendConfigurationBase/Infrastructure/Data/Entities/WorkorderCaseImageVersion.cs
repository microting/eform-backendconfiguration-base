using Microting.eFormApi.BasePn.Infrastructure.Database.Base;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class WorkorderCaseImageVersion : BaseEntity
{
    public int WorkorderCaseImageId { get; set; }

    public int WorkorderCaseId { get; set; }

    public int UploadedDataId { get; set; }
}