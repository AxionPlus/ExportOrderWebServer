using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Port.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Port.Mapper
{
    public static class PortMapper
    {
        public static PortDto ToDto(this PortBaseEntity entity)
        {
            if (entity == null) return null;

            return new PortDto
            {
                Id = entity.Id,
                NameRu = entity.NameRu,
                NameEn = entity.NameEn,
                CountryRu = entity.CountryRu,
                CountryEn = entity.CountryEn,
                IsoCode = entity.IsoCode,
                AuxIsoCode = entity.AuxIsoCode,
                PikYugIsoCode = entity.PikYugIsoCode,

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

        public static PortBaseEntity ToEntity(this PortDto dto)
        {
            if (dto == null) return null;

            return new PortBaseEntity
            {
                Id = dto.Id,
                NameRu = dto.NameRu,
                NameEn = dto.NameEn,
                CountryRu = dto.CountryRu,
                CountryEn = dto.CountryEn,
                IsoCode = dto.IsoCode,
                AuxIsoCode = dto.AuxIsoCode,
                PikYugIsoCode = dto.PikYugIsoCode,

                // BaseEntity fields
                Status = dto.Status,
            };
        }

        public static void UpdateEntity(this PortBaseEntity entity, PortDto dto)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            entity.NameRu = dto.NameRu;
            entity.NameEn = dto.NameEn;
            entity.CountryRu = dto.CountryRu;
            entity.CountryEn = dto.CountryEn;
            entity.IsoCode = dto.IsoCode;
            entity.AuxIsoCode = dto.AuxIsoCode;
            entity.PikYugIsoCode = dto.PikYugIsoCode;
            entity.Status = dto.Status;
        }

        public static IEnumerable<PortDto> ToDtoList(this IEnumerable<PortBaseEntity> entities)
        {
            if (entities == null) return Enumerable.Empty<PortDto>();

            return entities.Select(entity => entity.ToDto());
        }

        public static List<PortBaseEntity> ToEntityList(this IEnumerable<PortDto> dtos)
        {
            if (dtos == null) return new List<PortBaseEntity>();

            return dtos.Select(dto => dto.ToEntity()).ToList();
        }
    }
}
