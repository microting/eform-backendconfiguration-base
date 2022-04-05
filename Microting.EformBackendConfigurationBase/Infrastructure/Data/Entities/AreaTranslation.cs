using System.ComponentModel.DataAnnotations.Schema;

namespace Microting.EformBackendConfigurationBase.Infrastructure.Data.Entities
{
    public class AreaTranslation: PnBase
    {
        [ForeignKey("AreaId")]
        public int AreaId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int LanguageId { get; set; }

        public string InfoBox { get; set; }

        public string Placeholder { get; set; }

        public string NewItemName { get; set; }
    }
}