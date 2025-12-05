using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Mapper;
using Dto = ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Mapper;

public static class BillOfLadingMapper
{
    public static Dto.BillOfLadingDto ToDto(this BillOfLadingBaseEntity entity)
    {
        if (entity == null) return null;

        return new Dto.BillOfLadingDto
        {
            Id = entity.Id,
            Num = entity.Num,
            Date = entity.Date,
            TsDate = entity.TsDate,
            TsPort = entity.TsPort,
            CustomerCode = entity.CustomerCode,
            BookingParty = entity.BookingParty,
            Origin = entity.Origin,
            Pol = entity.Pol,
            Pod = entity.Pod,
            FinalPod = entity.FinalPod,
            Shipper = entity.Shipper,
            ShipperCode = entity.ShipperCode,
            ShipperName = entity.ShipperName,
            ShipperAddress = entity.ShipperAddress,
            ShipperNameRu = entity.ShipperNameRu,
            Consignee = entity.Consignee,
            ConsigneeCode = entity.ConsigneeCode,
            ConsigneeName = entity.ConsigneeName,
            ConsigneeAddress = entity.ConsigneeAddress,
            ConsigneeNameRu = entity.ConsigneeNameRu,
            ConsigneeAddressRu = entity.ConsigneeAddressRu,
            PartBl = entity.PartBl,
            CargoDescription = entity.CargoDescription,
            VesselCallId = entity.VesselCallId,
            VesselCall = entity.VesselCall.ToDto(),
            ContainerRecords = entity.ContainerRecords?.Select(cr => cr.ToDto()).ToList() ?? new(),

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

    public static Dto.BillOfLadingContainerRecordDto ToDto(this BillOfLadingContainerRecordBaseEntity entity)
    {
        if (entity == null) return null;

        return new Dto.BillOfLadingContainerRecordDto
        {
            Id = entity.Id,
            ContainerNo = entity.ContainerNo,
            ContainerTypeId = entity.ContainerTypeId,
            IsoCode = entity.IsoCode,
            TareWt = entity.TareWt,
            FullOrEmpty = entity.FullOrEmpty,
            IsSoc = entity.IsSoc,
            SealNo = entity.SealNo,
            PackageType = entity.PackageType,
            NoOfPackage = entity.NoOfPackage,
            GrossWeight = entity.GrossWeight,
            GrossWeightUOM = entity.GrossWeightUOM,
            Volume = entity.Volume,
            OutOfGauge = entity.OutOfGauge,
            IMCOClass = entity.IMCOClass,
            IMCONumber = entity.IMCONumber,
            ReeferTempSign = entity.ReeferTempSign,
            ReeferTemp = entity.ReeferTemp,
            ReeferTempUOM = entity.ReeferTempUOM,
            ReeferHumidity = entity.ReeferHumidity,
            ReeferVentilation = entity.ReeferVentilation,
            BookingNo = entity.BookingNo,
            ContainerAsCargo = entity.ContainerAsCargo,

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

    public static BillOfLadingBaseEntity ToEntity(this Dto.BillOfLadingDto dto)
    {
        if (dto == null) return null;

        return new BillOfLadingBaseEntity
        {
            Id = dto.Id,
            Num = dto.Num,
            Date = dto.Date.Value,
            TsDate = dto.TsDate,
            TsPort = dto.TsPort,
            CustomerCode = dto.CustomerCode,
            BookingParty = dto.BookingParty,
            Origin = dto.Origin,
            Pol = dto.Pol,
            Pod = dto.Pod,
            FinalPod = dto.FinalPod,
            Shipper = dto.Shipper,
            ShipperCode = dto.ShipperCode,
            ShipperName = dto.ShipperName,
            ShipperAddress = dto.ShipperAddress,
            ShipperNameRu = dto.ShipperNameRu,
            Consignee = dto.Consignee,
            ConsigneeCode = dto.ConsigneeCode,
            ConsigneeName = dto.ConsigneeName,
            ConsigneeAddress = dto.ConsigneeAddress,
            ConsigneeNameRu = dto.ConsigneeNameRu,
            ConsigneeAddressRu = dto.ConsigneeAddressRu,
            PartBl = dto.PartBl,
            CargoDescription = dto.CargoDescription,
            VesselCallId = dto.VesselCallId,
            ContainerRecords = dto.ContainerRecords?.Select(cr => cr.ToEntity()).ToList() ?? new(),
            HandledBySystem = dto.HandledBySystem,
            // BaseEntity fields
            Status = dto.Status,
        };
    }

    public static BillOfLadingContainerRecordBaseEntity ToEntity(this Dto.BillOfLadingContainerRecordDto dto)
    {
        if (dto == null) return null;

        return new BillOfLadingContainerRecordBaseEntity
        {
            Id = dto.Id,
           // ContainerNo = dto.ContainerNo,
            ContainerTypeId = dto.ContainerTypeId,
            IsoCode = dto.IsoCode,
            TareWt = dto.TareWt,
            FullOrEmpty = dto.FullOrEmpty,
            IsSoc = dto.IsSoc,
            SealNo = dto.SealNo,
            PackageType = dto.PackageType,
            NoOfPackage = dto.NoOfPackage,
            GrossWeight = dto.GrossWeight,
            GrossWeightUOM = dto.GrossWeightUOM,
            Volume = dto.Volume,
            OutOfGauge = dto.OutOfGauge,
            IMCOClass = dto.IMCOClass,
            IMCONumber = dto.IMCONumber,
            ReeferTempSign = dto.ReeferTempSign,
            ReeferTemp = dto.ReeferTemp,
            ReeferTempUOM = dto.ReeferTempUOM,
            ReeferHumidity = dto.ReeferHumidity,
            ReeferVentilation = dto.ReeferVentilation,
            BookingNo = dto.BookingNo,
            ContainerAsCargo = dto.ContainerAsCargo,
            HandledBySystem = dto.HandledBySystem,
            // BaseEntity fields
            Status = dto.Status,
        };
    }

    public static void UpdateEntity(this BillOfLadingBaseEntity entity, Dto.BillOfLadingDto dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        //entity.Num = dto.Num;
        entity.Date = dto.Date.Value;
        entity.TsDate = dto.TsDate;
        entity.TsPort = dto.TsPort;
        //entity.CustomerCode = dto.CustomerCode;
        //entity.BookingParty = dto.BookingParty;
        entity.Origin = dto.Origin;
        entity.Pol = dto.Pol;
        entity.Pod = dto.Pod;
        entity.FinalPod = dto.FinalPod;
        entity.Shipper = dto.Shipper;
        //entity.ShipperCode = dto.ShipperCode;
        entity.ShipperName = dto.ShipperName;
        entity.ShipperAddress = dto.ShipperAddress;
        entity.ShipperNameRu = dto.ShipperNameRu;
        entity.Consignee = dto.Consignee;
        //entity.ConsigneeCode = dto.ConsigneeCode;
        entity.ConsigneeName = dto.ConsigneeName;
        entity.ConsigneeAddress = dto.ConsigneeAddress;
        entity.ConsigneeNameRu = dto.ConsigneeNameRu;
        entity.ConsigneeAddressRu = dto.ConsigneeAddressRu;
       // entity.PartBl = dto.PartBl;
        entity.CargoDescription = dto.CargoDescription;
        entity.VesselCallId = dto.VesselCallId;
        entity.Status = dto.Status;

        // Обновляем контейнерные записи
        //if (dto.ContainerRecords != null)
        //{
        //    entity.ContainerRecords.Clear();
        //    foreach (var containerDto in dto.ContainerRecords)
        //    {
        //        var containerEntity = containerDto.ToEntity();
        //        entity.ContainerRecords.Add(containerEntity);
        //    }
        //}
    }

    public static IEnumerable<Dto.BillOfLadingDto> ToDtoList(this IEnumerable<BillOfLadingBaseEntity> entities)
    {
        if (entities == null) return Enumerable.Empty<Dto.BillOfLadingDto>();
        return entities.Select(entity => entity.ToDto());
    }

    public static List<BillOfLadingBaseEntity> ToEntityList(this IEnumerable<Dto.BillOfLadingDto> dtos)
    {
        if (dtos == null) return new List<BillOfLadingBaseEntity>();
        return dtos.Select(dto => dto.ToEntity()).ToList();
    }
}