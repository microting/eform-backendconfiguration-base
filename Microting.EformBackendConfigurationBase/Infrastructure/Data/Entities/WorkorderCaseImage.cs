namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities;

public class WorkorderCaseImage : PnBase
{
    public int WorkorderCaseId { get; set; }

    public virtual WorkorderCase WorkorderCase{ get; set; }

    public int UploadedDataId { get; set; }

    public virtual UploadedData UploadedData { get; set; }
}