namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities
{
    public class AreaTranslationVersion: PnBase
    {
        public int AreaId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int LanguageId { get; set; }

        public int AreaTranslationId { get; set; }
    }
}