using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Vessel.Mapper
{
    public static class VesselMapper
    {
        public static VesselDto ToDto(this VesselBaseEntity entity)
        {
            if (entity == null) return null;

            return new VesselDto
            {
                Id = entity.Id,
                Name = entity.Name,
                FlagRu = entity.FlagRu,
                FlagEn = entity.FlagEn,
                ShortName = entity.ShortName,
                RolisCode = entity.RolisCode,

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

        public static VesselBaseEntity ToEntity(this VesselDto dto)
        {
            if (dto == null) return null;

            return new VesselBaseEntity
            {
                Id = dto.Id,
                Name = dto.Name,
                FlagRu = dto.FlagRu,
                FlagEn = dto.FlagEn,
                ShortName = dto.ShortName,
                RolisCode = dto.RolisCode,

                // BaseEntity fields
                Status = dto.Status,

            };
        }

        public static void UpdateEntity(this VesselBaseEntity entity, VesselDto dto)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            entity.Name = dto.Name;
            entity.FlagRu = dto.FlagRu;
            entity.FlagEn = dto.FlagEn;
            entity.ShortName = dto.ShortName;
            entity.RolisCode = dto.RolisCode;
            entity.Status = dto.Status;
   
        }

        public static IEnumerable<VesselDto> ToDtoList(this IEnumerable<VesselBaseEntity> entities)
        {
            if (entities == null) return Enumerable.Empty<VesselDto>();

            return entities.Select(entity => entity.ToDto());
        }

        public static List<VesselBaseEntity> ToEntityList(this IEnumerable<VesselDto> dtos)
        {
            if (dtos == null) return new List<VesselBaseEntity>();

            return dtos.Select(dto => dto.ToEntity()).ToList();
        }
    }
}
