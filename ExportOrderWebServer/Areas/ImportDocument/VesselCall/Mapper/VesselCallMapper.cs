using DocumentFormat.OpenXml.Vml.Office;
using ExportOrderEntites;
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.Port.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.VesselCall.Mapper
{
    public static class VesselCallMapper
    {
        public static VesselCallDto ToDto(this VesselCallBaseEntity entity)
        {
            if (entity == null) return null;

            return new VesselCallDto
            {
                Id = entity.Id,
                IsFinalized = entity.IsFinalized,
                VesselId = entity.VesselId,
                Vessel = entity.Vessel.ToDto(), // Предполагая существование ToVesselDto
                VoyageNo = entity.VoyageNo,
                TerminalVoyageNo = entity.TerminalVoyageNo,
                TerminalId = entity.TerminalId,
                Terminal = entity.Terminal?.ToDto(), // Нужно создать этот метод
                ETA = entity.ETA,
                ETS = entity.ETS,
                PortOfLoadingId = entity.PortOfLoadingId,
                PortOfLoading = entity.PortOfLoading.ToDto(), // Предполагая существование ToPortDto
                FeederBlNo = entity.FeederBlNo,
                GoogleTableUrl = entity.GoogleTableUrl,
                PortOfLoadingDate = entity.PortOfLoadingDate,

                CaptainName = entity.CaptainName,
                CaptainLastName = entity.CaptainLastName,
                TranslateUpdateTime = entity.TranslateUpdateTime,

                BillOfLadings = entity.BillOfLadings.Any() ? entity.BillOfLadings.ToDtoList() : new(),
                // BaseEntity fields
                Status = entity.Status,
                Version = entity.Version,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                DeletedBy = entity.DeletedBy,
                DeleteReason = entity.DeleteReason,
                HandledBySystem = entity.HandledBySystem,
                LockToken = entity.LockToken,
                Timestamp = entity.Timestamp
            };
        }

        public static VesselCallBaseEntity ToEntity(this VesselCallDto dto)
        {
            if (dto == null) return null;

            return new VesselCallBaseEntity
            {
                Id = dto.Id,
                IsFinalized = dto.IsFinalized,
                VesselId = dto.VesselId,
                VoyageNo = dto.VoyageNo,
                TerminalVoyageNo = dto.TerminalVoyageNo,
                TerminalId = dto.TerminalId,
                GoogleTableUrl = dto.GoogleTableUrl,
                ETA = dto.ETA.Value,
                ETS = dto.ETS.Value,
                PortOfLoadingDate = dto.PortOfLoadingDate,
                PortOfLoadingId = dto.PortOfLoadingId,
                FeederBlNo = dto.FeederBlNo,

                CaptainName = dto.CaptainName,
                CaptainLastName = dto.CaptainLastName,
                TranslateUpdateTime = dto.TranslateUpdateTime,

                // BaseEntity fields
                Status = dto.Status,
            };
        }

        public static void UpdateEntity(this VesselCallBaseEntity entity, VesselCallDto dto)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            entity.IsFinalized = dto.IsFinalized;
            entity.VesselId = dto.VesselId;
            entity.VoyageNo = dto.VoyageNo;
            entity.TerminalVoyageNo = dto.TerminalVoyageNo;
            entity.TerminalId = dto.TerminalId;
            entity.ETA = dto.ETA.Value;
            entity.ETS = dto.ETS.Value;
            entity.PortOfLoadingDate = dto.PortOfLoadingDate;
            entity.PortOfLoadingId = dto.PortOfLoadingId;
            entity.FeederBlNo = dto.FeederBlNo;
            entity.Status = dto.Status;
            entity.GoogleTableUrl = dto.GoogleTableUrl;
            entity.CaptainName = dto.CaptainName;
            entity.CaptainLastName = dto.CaptainLastName;
            entity.TranslateUpdateTime = dto.TranslateUpdateTime;
        }




        public static IEnumerable<VesselCallDto> ToDtoList(this IEnumerable<VesselCallBaseEntity> entities)
        {
            if (entities == null) return Enumerable.Empty<VesselCallDto>();

            return entities.Select(entity => entity.ToDto());
        }

        public static List<VesselCallBaseEntity> ToEntityList(this IEnumerable<VesselCallDto> dtos)
        {
            if (dtos == null) return new List<VesselCallBaseEntity>();

            return dtos.Select(dto => dto.ToEntity()).ToList();
        }
    }
}