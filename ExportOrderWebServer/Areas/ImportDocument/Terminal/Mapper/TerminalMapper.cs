using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Terminal.Mapper
{
    public static class TerminalMapper
    {
        public static TerminalDto ToDto(this TerminalBaseEntity entity)
        {
            if (entity == null) return null;

            return new TerminalDto
            {
                Id = entity.Id,
                Name = entity.Name,
                NameRu = entity.NameRu,
                Email = entity.Email,
                CustomsPost = entity.CustomsPost,
                CustomsPostName = entity.CustomsPostName,
                ContractId = entity.ContractId,
                ContractName = entity.ContractName,

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

        public static TerminalBaseEntity ToEntity(this TerminalDto dto)
        {
            if (dto == null) return null;

            return new TerminalBaseEntity
            {
                Id = dto.Id,
                Name = dto.Name,
                NameRu = dto.NameRu,
                Email = dto.Email,
                CustomsPost = dto.CustomsPost,
                CustomsPostName = dto.CustomsPostName,
                ContractId = dto.ContractId,
                ContractName = dto.ContractName,

                // BaseEntity fields
                Status = dto.Status,
            };
        }

        public static void UpdateEntity(this TerminalBaseEntity entity, TerminalDto dto)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            entity.Name = dto.Name;
            entity.NameRu = dto.NameRu;
            entity.Email = dto.Email;
            entity.CustomsPost = dto.CustomsPost;
            entity.CustomsPostName = dto.CustomsPostName;
            entity.ContractId = dto.ContractId;
            entity.ContractName = dto.ContractName;
            entity.Status = dto.Status;
        }

        public static IEnumerable<TerminalDto> ToDtoList(this IEnumerable<TerminalBaseEntity> entities)
        {
            if (entities == null) return Enumerable.Empty<TerminalDto>();

            return entities.Select(entity => entity.ToDto());
        }

        public static List<TerminalBaseEntity> ToEntityList(this IEnumerable<TerminalDto> dtos)
        {
            if (dtos == null) return new List<TerminalBaseEntity>();

            return dtos.Select(dto => dto.ToEntity()).ToList();
        }
    }
}