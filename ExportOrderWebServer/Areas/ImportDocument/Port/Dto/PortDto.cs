using ExportOrderEntites.ImportDocument;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExportOrderWebServer.Areas.ImportDocument.Port.Dto
{
    public class PortDto : BaseEntity
    {
        public string NameRu { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string CountryRu { get; set; } = string.Empty;
        public string CountryEn { get; set; } = string.Empty;
        public string IsoCode { get; set; } = string.Empty;
        public string AuxIsoCode { get; set; } = string.Empty;
        public string PikYugIsoCode { get; set; } = string.Empty;

        [NotMapped]
        public string FullEn => $"{NameEn}, {CountryEn}";
        [NotMapped]
        public string FullRu => $"{NameRu}, {CountryRu}";

        public bool IsNew => Timestamp == 0;
    }
}
