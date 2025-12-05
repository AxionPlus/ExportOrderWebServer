using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Customer.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Customer.Mapper
{
    public static class CustomerMapper
    {
        public static CustomerDto ToDto(this CustomerBaseEntity entity)
        {
            if (entity == null) return null;

            return new CustomerDto
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Code = entity.Code,
                Name = entity.Name,
                Address = entity.Address,
                NameRu = entity.NameRu,
                AddressRu = entity.AddressRu,

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

        public static CustomerBaseEntity ToEntity(this CustomerDto dto)
        {
            if (dto == null) return null;

            return new CustomerBaseEntity
            {
                Id = dto.Id,
                FullName = dto.FullName,
                Code = dto.Code,
                Name = dto.Name,
                Address = dto.Address,
                NameRu = dto.NameRu,
                AddressRu = dto.AddressRu,

                // BaseEntity fields
                Status = dto.Status,
            };
        }

        public static void UpdateEntity(this CustomerBaseEntity entity, CustomerDto dto)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            entity.FullName = dto.FullName;
            entity.Code = dto.Code;
            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.NameRu = dto.NameRu;
            entity.AddressRu = dto.AddressRu;
            entity.Status = dto.Status;
        }

        public static IEnumerable<CustomerDto> ToDtoList(this IEnumerable<CustomerBaseEntity> entities)
        {
            if (entities == null) return Enumerable.Empty<CustomerDto>();

            return entities.Select(entity => entity.ToDto());
        }

        public static List<CustomerBaseEntity> ToEntityList(this IEnumerable<CustomerDto> dtos)
        {
            if (dtos == null) return new List<CustomerBaseEntity>();

            return dtos.Select(dto => dto.ToEntity()).ToList();
        }
    }
}