using ExportOrderEntites;
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Port.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Mapper;

namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Mapper;

public static class BillOfLadingMapper
{
    public static Dto.BillOfLadingBaseDto ToDto(this BillOfLadingBaseEntity entity)
    {
        if (entity == null) return null;

        return new Dto.BillOfLadingBaseDto
        {
            Id = entity.Id,
            Num = entity.Num,
            Date = entity.Date,
            TsDate = entity.TsDate,
            TsPort = entity.TsPort?.ToDto(),
            Carrier = entity.Carrier,
            CustomerCode = entity.CustomerCode,
            BookingParty = entity.BookingParty,
            Origin = entity.Origin,
            Pol = entity.Pol?.ToDto(),
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
            ConsigneeCountryRu = entity.ConsigneeCountryRu,
            PartBl = entity.PartBl,
            CargoDescription = entity.CargoDescription,
            CargoDescriptionRu = entity.CargoDescriptionRu,
            CustomsMode = entity.CustomsMode,
            VesselCallId = entity.VesselCallId,
            // VesselCall = entity.VesselCall.ToDto(),
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

    public static Dto.BillOfLadingContainerRecordBaseDto ToDto(this BillOfLadingContainerRecordBaseEntity entity)
    {
        if (entity == null) return null;

        return new Dto.BillOfLadingContainerRecordBaseDto
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
            GrossWeight =  entity.GrossWeight,
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
            CargoDescriptionRu = entity.CargoDescriptionRu,
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

    public static BillOfLadingBaseEntity ToEntity(this Dto.BillOfLadingBaseDto baseDto)
    {
        if (baseDto == null) return null;

        return new BillOfLadingBaseEntity
        {
            Id = baseDto.Id,
            Num = baseDto.Num,
            Date = baseDto.Date.Value,
            TsDate = baseDto.TsDate,
            TsPortId = baseDto.TsPort?.Id,
            Carrier = baseDto.Carrier,
            CustomerCode = baseDto.CustomerCode,
            BookingParty = baseDto.BookingParty,
            Origin = baseDto.Origin,
            PolId = baseDto.Pol?.Id,
            Pod = baseDto.Pod,
            FinalPod = baseDto.FinalPod,
            Shipper = baseDto.Shipper,
            ShipperCode = baseDto.ShipperCode,
            ShipperName = baseDto.ShipperName,
            ShipperAddress = baseDto.ShipperAddress,
            ShipperNameRu = baseDto.ShipperNameRu,
            Consignee = baseDto.Consignee,
            ConsigneeCode = baseDto.ConsigneeCode,
            ConsigneeName = baseDto.ConsigneeName,
            ConsigneeAddress = baseDto.ConsigneeAddress,
            ConsigneeNameRu = baseDto.ConsigneeNameRu,
            ConsigneeAddressRu = baseDto.ConsigneeAddressRu,
            ConsigneeCountryRu = baseDto.ConsigneeCountryRu,
            PartBl = baseDto.PartBl,
            CargoDescription = baseDto.CargoDescription,
            CargoDescriptionRu = baseDto.CargoDescriptionRu,
            VesselCallId = baseDto.VesselCallId,
            ContainerRecords = baseDto.ContainerRecords?.Select(cr => cr.ToEntity()).ToList() ?? new(),
            HandledBySystem = baseDto.HandledBySystem,
            // BaseEntity fields
            Status = baseDto.Status,
        };
    }

    public static BillOfLadingContainerRecordBaseEntity ToEntity(this Dto.BillOfLadingContainerRecordBaseDto baseDto)
    {
        if (baseDto == null) return null;

        return new BillOfLadingContainerRecordBaseEntity
        {
            Id = baseDto.Id,
            ContainerNo = baseDto.ContainerNo,
            ContainerTypeId = baseDto.ContainerTypeId,
            IsoCode = baseDto.IsoCode,
            TareWt = baseDto.TareWt,
            FullOrEmpty = baseDto.FullOrEmpty,
            IsSoc = baseDto.IsSoc,
            SealNo = baseDto.SealNo,
            PackageType = baseDto.PackageType,
            NoOfPackage = baseDto.NoOfPackage,
            GrossWeight = baseDto.GrossWeight,
            GrossWeightUOM = baseDto.GrossWeightUOM,
            Volume = baseDto.Volume,
            OutOfGauge = baseDto.OutOfGauge,
            IMCOClass = baseDto.IMCOClass,
            IMCONumber = baseDto.IMCONumber,
            ReeferTempSign = baseDto.ReeferTempSign,
            ReeferTemp = baseDto.ReeferTemp,
            ReeferTempUOM = baseDto.ReeferTempUOM,
            ReeferHumidity = baseDto.ReeferHumidity,
            ReeferVentilation = baseDto.ReeferVentilation,
            BookingNo = baseDto.BookingNo,
            ContainerAsCargo = baseDto.ContainerAsCargo,
            HandledBySystem = baseDto.HandledBySystem,
            // BaseEntity fields
            Status = baseDto.Status,
        };
    }

    public static void UpdateEntity(this BillOfLadingBaseEntity entity, Dto.BillOfLadingBaseDto baseDto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (baseDto == null) throw new ArgumentNullException(nameof(baseDto));

        //entity.Num = dto.Num;
        entity.Date = baseDto.Date.Value;
        entity.TsDate = baseDto.TsDate;
        entity.TsPortId = baseDto.TsPort?.Id;
        //entity.CustomerCode = dto.CustomerCode;
        //entity.BookingParty = dto.BookingParty;
        entity.Origin = baseDto.Origin;
        entity.PolId = baseDto.Pol?.Id;
        entity.Pod = baseDto.Pod;
        entity.FinalPod = baseDto.FinalPod;
        entity.Shipper = baseDto.Shipper;
        //entity.ShipperCode = dto.ShipperCode;
        entity.ShipperName = baseDto.ShipperName;
        entity.ShipperAddress = baseDto.ShipperAddress;
        entity.ShipperNameRu = baseDto.ShipperNameRu;
        entity.Consignee = baseDto.Consignee;
        //entity.ConsigneeCode = dto.ConsigneeCode;
        entity.ConsigneeName = baseDto.ConsigneeName;
        entity.ConsigneeAddress = baseDto.ConsigneeAddress;
        entity.ConsigneeNameRu = baseDto.ConsigneeNameRu;
        entity.ConsigneeAddressRu = baseDto.ConsigneeAddressRu;
        entity.ConsigneeCountryRu = baseDto.ConsigneeCountryRu;
        // entity.PartBl = dto.PartBl;
        entity.CargoDescription = baseDto.CargoDescription;
        entity.CargoDescriptionRu = baseDto.CargoDescriptionRu;
        entity.VesselCallId = baseDto.VesselCallId;
        entity.Status = baseDto.Status;
        entity.CustomsMode = baseDto.CustomsMode;

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
    public static void ReUpdateEntity(this BillOfLadingBaseEntity entity, Dto.BillOfLadingBaseDto baseDto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (baseDto == null) throw new ArgumentNullException(nameof(baseDto));

        entity.Date = baseDto.Date.Value;
        entity.CustomerCode = baseDto.CustomerCode;
        entity.BookingParty = baseDto.BookingParty;
        entity.Origin = baseDto.Origin;
        entity.PolId = baseDto.Pol?.Id;
        entity.Pod = baseDto.Pod;
        entity.FinalPod = baseDto.FinalPod;
        entity.Shipper = baseDto.Shipper;
        entity.ShipperCode = baseDto.ShipperCode;
        entity.ShipperName = baseDto.ShipperName;
        entity.ShipperAddress = baseDto.ShipperAddress;
        entity.Consignee = baseDto.Consignee;
        entity.ConsigneeCode = baseDto.ConsigneeCode;
        entity.ConsigneeName = baseDto.ConsigneeName;
        entity.ConsigneeAddress = baseDto.ConsigneeAddress;
        entity.PartBl = baseDto.PartBl;
        entity.CargoDescription = baseDto.CargoDescription;
        entity.CustomsMode = baseDto.CustomsMode;

        //Обновляем контейнерные записи
        if (baseDto.ContainerRecords.Any())
        {
            entity.ContainerRecords.Clear();
            foreach (var containerDto in baseDto.ContainerRecords)
            {
                entity.ContainerRecords.Add(containerDto.ToEntity());
            }
        }
    }
    public static void ReUpdateEntity(this BillOfLadingContainerRecordBaseEntity entity, Dto.BillOfLadingContainerRecordBaseDto baseDto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (baseDto == null) throw new ArgumentNullException(nameof(baseDto));

        entity.ContainerTypeId = baseDto.ContainerTypeId;
        entity.IsoCode = baseDto.IsoCode;
        entity.TareWt = baseDto.TareWt;
        entity.FullOrEmpty = baseDto.FullOrEmpty;
        entity.IsSoc = baseDto.IsSoc;
        entity.SealNo = baseDto.SealNo;
        entity.PackageType = baseDto.PackageType;
        entity.NoOfPackage = baseDto.NoOfPackage;
        entity.GrossWeight = baseDto.GrossWeight;
        entity.GrossWeightUOM = baseDto.GrossWeightUOM;
        entity.Volume = baseDto.Volume;
        entity.OutOfGauge = baseDto.OutOfGauge;
        entity.IMCOClass = baseDto.IMCOClass;
        entity.IMCONumber = baseDto.IMCONumber;
        entity.ReeferTempSign = baseDto.ReeferTempSign;
        entity.ReeferTemp = baseDto.ReeferTemp;
        entity.ReeferTempUOM = baseDto.ReeferTempUOM;
        entity.ReeferHumidity = baseDto.ReeferHumidity;
        entity.ReeferVentilation = baseDto.ReeferVentilation;
        entity.ContainerAsCargo = baseDto.ContainerAsCargo;
        entity.HandledBySystem = baseDto.HandledBySystem;
    }
    public static void UpdateEntity(this BillOfLadingContainerRecordBaseEntity entity, Dto.BillOfLadingContainerRecordBaseDto baseDto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (baseDto == null) throw new ArgumentNullException(nameof(baseDto));

        entity.ContainerTypeId = baseDto.ContainerTypeId;
        entity.IsoCode = baseDto.IsoCode;
        entity.TareWt = baseDto.TareWt;
        entity.FullOrEmpty = baseDto.FullOrEmpty;
        entity.IsSoc = baseDto.IsSoc;
        entity.SealNo = baseDto.SealNo;
        entity.PackageType = baseDto.PackageType;
        entity.NoOfPackage = baseDto.NoOfPackage;
        entity.GrossWeight = baseDto.GrossWeight;
        entity.GrossWeightUOM = baseDto.GrossWeightUOM;
        entity.Volume = baseDto.Volume;
        entity.OutOfGauge = baseDto.OutOfGauge;
        entity.IMCOClass = baseDto.IMCOClass;
        entity.IMCONumber = baseDto.IMCONumber;
        entity.ReeferTempSign = baseDto.ReeferTempSign;
        entity.ReeferTemp = baseDto.ReeferTemp;
        entity.ReeferTempUOM = baseDto.ReeferTempUOM;
        entity.ReeferHumidity = baseDto.ReeferHumidity;
        entity.ReeferVentilation = baseDto.ReeferVentilation;
        entity.ContainerAsCargo = baseDto.ContainerAsCargo;
        entity.HandledBySystem = baseDto.HandledBySystem;
        entity.CargoDescriptionRu = baseDto.CargoDescriptionRu;
        entity.Status = baseDto.Status;
    }


    public static List<BillOfLadingBaseDto> ToDtoList(this IEnumerable<BillOfLadingBaseEntity> entities)
    {
        return entities.Select(entity => entity.ToDto()).ToList();
    }

    public static List<BillOfLadingBaseEntity> ToEntityList(this IEnumerable<Dto.BillOfLadingBaseDto> dtos)
    {
        if (dtos == null) return new List<BillOfLadingBaseEntity>();
        return dtos.Select(dto => dto.ToEntity()).ToList();
    }
}