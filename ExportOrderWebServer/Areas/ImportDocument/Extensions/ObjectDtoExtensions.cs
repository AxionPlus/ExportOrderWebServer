using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Extensions
{
    public static class ObjectDtoExtensions
    {
        public static ObjectDto? ToObjectDto(this PortBaseEntity? obj)
        {
            return obj == null ? null : new ObjectDto(obj.Id, obj.NameEn, obj.IsoCode);
        }
        public static ObjectDto? ToObjectDto(this VesselBaseEntity? obj)
        {
            return obj == null ? null : new ObjectDto(obj.Id, obj.Name, obj.ShortName);
        }
        public static ObjectDto? ToObjectDto(this TerminalBaseEntity? obj)
        {
            return obj == null ? null : new ObjectDto(obj.Id, obj.Name);
        }


    }
}
