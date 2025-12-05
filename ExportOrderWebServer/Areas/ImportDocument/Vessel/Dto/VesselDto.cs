using ExportOrderEntites.ImportDocument;

namespace ExportOrderWebServer.Areas.ImportDocument.Vessel.Dto
{
    public class VesselDto : BaseEntity
    {
        
        public string Name { get; set; } = string.Empty;
        public string FlagRu { get; set; } = string.Empty;
        public string FlagEn { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string RolisCode { get; set; } = string.Empty;


        public bool IsNew => Timestamp == 0;
    }
}
