using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
using System.Xml.Serialization;

namespace ExportOrderWebServer.Areas.ImportDocument.Services
{
    public interface IXmlExportService
    {
        byte[] CreateManifestXml(VesselCallDto vesselCall, string? billOfLadingNum = "");

    }

    public class XmlExportService : IXmlExportService
    {
        public byte[] CreateManifestXml(VesselCallDto vesselCall, string? billOfLadingNum = "")
        {
            // Определяем формат на основе терминала
            var terminalName = vesselCall.Terminal?.Name?.ToUpper() ?? "";

            if (terminalName.Contains("NUTEP") || terminalName.Contains("НУТЭП"))
            {
                return CreateNutepFormatXml(vesselCall, billOfLadingNum);
            }
            else if (terminalName.Contains("NLE"))
            {
                return CreateNleFormatXml(vesselCall, billOfLadingNum);
            }
            else
            {
                // Формат по умолчанию (NUTEP)
                return CreateNutepFormatXml(vesselCall, billOfLadingNum);
            }
        }

        private byte[] CreateNutepFormatXml(VesselCallDto vesselCall, string? billOfLadingNum = "")
        {
            var manifest = new NutepManifest
            {
                DocHead = new DocHeadType
                {
                    DocName = "Manifest",
                    DocNumber = $"{DateTime.UtcNow:yyyyMMddmmss}",
                    DocDate = DateTime.UtcNow,
                    Modification = "create"
                },
                Line = "HUBSHIPPING",
                Carrier = "ALPHA",
                PartiesList = new PartiesListType
                {
                    Parties = new List<PartyType>
                    {
                        new PartyType
                        {
                            PartyFunction = "Agent",
                            Name = vesselCall.Terminal.ContractName,
                            ContractID = vesselCall.Terminal.ContractId,
                        },
                        new PartyType
                        {
                            PartyFunction = "Stevedore",
                            Name = vesselCall.Terminal?.NameRu?? throw new NullReferenceException("Terminal Name is null")
                        }
                    }
                },
                Arrival = new NutepArrivalType
                {
                    VesselID = vesselCall.Vessel?.RolisCode ?? throw new NullReferenceException("Vessel Rolis Code is null"),
                    Vessel = vesselCall.Vessel?.Name ?? throw new NullReferenceException("Vessel Name is null"),
                    ArrivalDate = vesselCall.ETA ?? DateTime.UtcNow
                },
                BLsList = new BLsListType
                {
                    BLs = new List<BLType>()
                }
            };

            // Добавляем коносаменты
            foreach (var billOfLading in vesselCall.BillOfLadings.Where(s => string.IsNullOrWhiteSpace(billOfLadingNum) || s.Num == billOfLadingNum))
            {
                var blType = new BLType
                {
                    DocHead = new BLDocHeadType
                    {
                        DocName = "Коносамент",
                        DocNumber = billOfLading.Num,
                        DocDate = billOfLading.TsDate ?? throw new NullReferenceException($"billOfLading {billOfLading.Num} TsDate is null"),
                        Modification = "create",
                        HouseNumber = vesselCall.FeederBlNo,
                        Carrier = billOfLading.Carrier ?? throw new NullReferenceException($"billOfLading {billOfLading.Num} Carrier is null"),
                    },
                    PartiesList = new BLPartiesListType
                    {
                        Parties = new List<BLPartyType>
                        {
                            new BLPartyType
                            {
                                PartyFunction = "Shipper",
                                Name = billOfLading.ShipperNameRu ?? throw new NullReferenceException($"billOfLading {billOfLading.Num} ShipperNameRu is null"),
                            },
                            new BLPartyType
                            {
                                PartyFunction = "Consignee",
                                Name = billOfLading.ConsigneeNameRu ??throw new NullReferenceException($"billOfLading {billOfLading.Num} ConsigneeNameRu is null"),
                            },
                            new BLPartyType
                            {
                                PartyFunction = "Notify",
                                Name = billOfLading.ConsigneeNameRu ?? throw new NullReferenceException($"billOfLading {billOfLading.Num} ConsigneeNameRu is null"),
                            }
                        }
                    },
                    OceanLineID = "597342273",
                    OriginID = billOfLading.TsPort?.AuxIsoCode ?? throw new NullReferenceException($"billOfLading {billOfLading.Num} TsPort AuxIsoCode is null"),
                    ContainerList = new ContainerListType
                    {
                        Containers = new List<ContainerType>()
                    }
                };

                // Добавляем контейнеры
                AddContainersToBL(blType, billOfLading);

                manifest.BLsList.BLs.Add(blType);
            }

            return SerializeToXml(manifest);
        }

        private byte[] CreateNleFormatXml(VesselCallDto vesselCall, string? billOfLadingNum = "")
        {
            var manifest = new NleManifest
            {
                DocHead = new DocHeadType
                {
                    DocName = "Manifest",
                    DocNumber = $"Manifest_HUBSHIPPING_{vesselCall.Vessel?.Name?.Replace(" ", "_")}_{vesselCall.VoyageNo}_{DateTime.UtcNow:dd.MM.yyyy}",
                    DocDate = DateTime.UtcNow,
                    Modification = "create"
                },
                Line = "ALPHA_LTD",
                PartiesList = new PartiesListType
                {
                    Parties = new List<PartyType>
                    {
                        new PartyType
                        {
                            PartyFunction = "Agent",
                            Name = vesselCall.Terminal.ContractName,
                            ContractID = vesselCall.Terminal.ContractId,
                        },
                        new PartyType
                        {
                            PartyFunction = "Stevedore",
                            Name = "NLE"
                        }
                    }
                },
                Arrival = new NleArrivalType
                {
                    VesselID = vesselCall.Vessel?.Name ?? throw new NullReferenceException("VesselID is null"),
                    Vessel = vesselCall.Vessel?.Name ?? throw new NullReferenceException("Vessel Name is null"),
                    ArrivalDate = vesselCall.ETA ?? DateTime.UtcNow,
                    VoyageNoIn = vesselCall.VoyageNo
                },
                BLsList = new NleBLsListType
                {
                    BLs = new List<NleBLType>()
                }
            };

            // Добавляем коносаменты
            foreach (var billOfLading in vesselCall.BillOfLadings.Where(s => string.IsNullOrWhiteSpace(billOfLadingNum) || s.Num == billOfLadingNum))
            {
                var blType = new NleBLType
                {
                    DocHead = new BLDocHeadType
                    {
                        DocName = "Коносамент",
                        DocNumber = billOfLading.Num,
                        DocDate = billOfLading.TsDate ?? throw new NullReferenceException($"billOfLading {billOfLading.Num} TsDate is null"),
                    },
                    PartiesList = new NleBLPartiesListType
                    {
                        Parties = new List<NleBLPartyType>
                        {
                            new NleBLPartyType
                            {
                                PartyFunction = "Shipper",
                                Name = billOfLading.ShipperNameRu ?? billOfLading.Shipper,
                                Country = billOfLading.Pol?.CountryRu ?? throw new NullReferenceException($"Shipper Country is null - {billOfLading}"),
                                Address = billOfLading.Pol?.NameRu ?? throw new NullReferenceException($"Shipper Address is null  - {billOfLading}")
                            },
                            new NleBLPartyType
                            {
                                PartyFunction = "Consignee",
                                Name = billOfLading.ConsigneeNameRu ?? throw new NullReferenceException($"Consignee Name is null - {billOfLading}"),
                                Address = billOfLading.ConsigneeAddressRu ?? throw new NullReferenceException($"Consignee Address is null - {billOfLading}"),
                                Country = billOfLading.ConsigneeCountryRu ??  throw new NullReferenceException($"Consignee Country is null - {billOfLading}"),
                            }
                        }
                    },
                    IsDirect = "0",
                    OriginID = billOfLading.TsPort?.NameEn ?? throw new NullReferenceException("OriginID POL is null"),
                    ContainerList = new NleContainerListType
                    {
                        Containers = new List<NleContainerType>()
                    }
                };

                // Добавляем контейнеры
                AddNleContainersToBL(blType, billOfLading);

                manifest.BLsList.BLs.Add(blType);
            }

            return SerializeToXml(manifest);
        }

        private void AddContainersToBL(BLType blType, BillOfLadingBaseDto billOfLading)
        {
            foreach (var container in billOfLading.ContainerRecords)
            {
                if (container.ContainerAsCargo)
                {
                    container.GrossWeight += container.TareWt;
                    container.TareWt = 0;

                    container.NoOfPackage += 1;
                }

                var containerType = new ContainerType
                {
                    Prefix = GetContainerPrefix(container.ContainerNo),
                    Number = GetContainerNumber(container.ContainerNo),
                    ISOType = container.IsoCode,
                    IsEmpty = container.FullOrEmpty?.ToUpper() == "E",
                    TareWeight = container.TareWt,
                    CargoName = container.CargoDescriptionRu ?? throw new NullReferenceException($"CargoDescriptionRu is null - {billOfLading.Num}/{container.ContainerNo}"),
                    CargoPlaces = container.NoOfPackage.ToString(),
                    CargoWeight = FormatWeight(container.GrossWeight),
                    SealList = new SealListType
                    {
                        Seals = ParseSealList(container.SealNo)
                    }
                };

                // Добавляем температурные условия для рефрижераторов
                if (!string.IsNullOrEmpty(container.ReeferTemp))
                {
                    containerType.TemperatureCondition = new TemperatureConditionType
                    {
                        Value = GetTemperatureValue(container),
                        Unit = "C"
                    };
                }

                // Добавляем информацию об опасных грузах
                if (!string.IsNullOrEmpty(container.IMCOClass) || !string.IsNullOrEmpty(container.IMCONumber))
                {
                    containerType.IMOList = new IMOListType
                    {
                        IMOCodes = [container.IMCOClass],
                    };
                }

                blType.ContainerList.Containers.Add(containerType);
            }
        }

        private void AddNleContainersToBL(NleBLType blType, BillOfLadingBaseDto billOfLading)
        {
            foreach (var container in billOfLading.ContainerRecords)
            {

                NleContainerType containerType;

                if (container.FullOrEmpty?.ToUpper() == "E")
                {
                     containerType = new NleContainerType
                    {
                        Prefix = GetContainerPrefix(container.ContainerNo),
                        Number = GetContainerNumber(container.ContainerNo),
                        ISOType = container.IsoCode,
                        IsEmpty = container.FullOrEmpty?.ToUpper() == "E",
                        TareWeight = FormatNleWeight(container.TareWt),
                    };
                }
                else
                {
                     containerType = new NleContainerType
                    {
                        Prefix = GetContainerPrefix(container.ContainerNo),
                        Number = GetContainerNumber(container.ContainerNo),
                        ISOType = container.IsoCode,
                        IsEmpty = container.FullOrEmpty?.ToUpper() == "E",
                        TareWeight = FormatNleWeight(container.TareWt),
                        CargoName = container.CargoDescriptionRu ?? throw new NullReferenceException($"CargoDescriptionRu is null - {billOfLading}/{container.ContainerNo}"),
                        oversized = container.OutOfGauge ? "true" : "false",
                        NumberOfUnits = container.NoOfPackage.ToString(),
                        CargoWeight = FormatNleWeight(container.GrossWeight),
                        SealList = new SealListType
                        {
                            Seals = ParseSealList(container.SealNo)
                        },

                        CustomKindPlanOut = billOfLading.CustomsMode ?? "ГТД"
                    };
                }
                // Добавляем температурные условия для рефрижераторов
                if (!string.IsNullOrEmpty(container.ReeferTemp))
                {
                    containerType.TemperatureCondition = new TemperatureConditionType
                    {
                        Value = GetTemperatureValue(container),
                        Unit = "C"
                    };
                }

                // Добавляем информацию об опасных грузах
                if (!string.IsNullOrEmpty(container.IMCOClass) || !string.IsNullOrEmpty(container.IMCONumber))
                {
                    containerType.IMOList = new IMOListType
                    {
                        IMOCodes = [container.IMCOClass],
                    };
                }

                // Добавляем UN коды опасности
                if (!string.IsNullOrEmpty(container.IMCONumber))
                {
                    containerType.UNHazardCodes = container.IMCONumber;
                }

                if (container.ContainerAsCargo)
                    containerType.instruction = "equipment";

                blType.ContainerList.Containers.Add(containerType);
            }
        }

        private byte[] SerializeToXml<T>(T obj)
        {
            var serializer = new XmlSerializer(typeof(T));

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, Encoding.GetEncoding("windows-1251"));
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            serializer.Serialize(streamWriter, obj, namespaces);
            streamWriter.Flush();
            return memoryStream.ToArray();
        }
        // Генерация номера документа
        private string GenerateDocNumber(VesselCallDto vesselCall) => $"{DateTime.UtcNow:yyyyMMdd}_{vesselCall.Vessel?.Name?.Replace(" ", "_")}_{vesselCall.VoyageNo}";

        private string GetContainerPrefix(string containerNo)
        {
            return !string.IsNullOrEmpty(containerNo) && containerNo.Length >= 4
                ? containerNo.Substring(0, 4)
                : throw new NullReferenceException($"Get Container Prefix error {containerNo}");
        }

        private string GetContainerNumber(string containerNo)
        {
            return !string.IsNullOrEmpty(containerNo) && containerNo.Length > 4
                ? containerNo.Substring(4)
                : throw new NullReferenceException($"Get Container Number error {containerNo}");
        }


        // Форматирование веса с запятой в качестве разделителя
        private string FormatWeight(double weight) => weight.ToString("F3").Replace(".", ",");

        // Для NLE формат веса без десятичных знаков

        private string FormatNleWeight(int tareWeight) => ((double)tareWeight).ToString("F0");
        private string FormatNleWeight(double weight) => weight.ToString("F3").Replace(",", ".");


        private string GetTemperatureValue(BillOfLadingContainerRecordBaseDto container)
        {
            if (!string.IsNullOrEmpty(container.ReeferFullTemp))
                return container.ReeferFullTemp;

            if (!string.IsNullOrEmpty(container.ReeferTempSign) && !string.IsNullOrEmpty(container.ReeferTemp))
                return $"{container.ReeferTempSign}{container.ReeferTemp}";

            return container.ReeferTemp ?? "0";
        }

        public List<string> ParseSealList(string sealString)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(sealString))
                return result;

            // Удаляем все пробелы и разбиваем по разделителям
            var cleanString = sealString.Replace(" ", "");
            var separators = new[] { ',', ';', ':', '/' };

            foreach (var part in cleanString.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    result.Add(part);
                }
            }

            // Если после разделения осталась только одна запись, возвращаем ее
            if (result.Count == 0 && !string.IsNullOrWhiteSpace(sealString.Trim()))
            {
                result.Add(sealString.Trim());
            }

            return result;
        }

    }

    // Классы для формата NUTEP
    [XmlRoot("Manifest")]
    public class NutepManifest
    {
        public DocHeadType DocHead { get; set; }
        public string Line { get; set; }
        public string Carrier { get; set; }
        public PartiesListType PartiesList { get; set; }
        public NutepArrivalType Arrival { get; set; }
        public BLsListType BLsList { get; set; }
    }

    // Классы для формата NLE
    [XmlRoot("Manifest")]
    public class NleManifest
    {
        public DocHeadType DocHead { get; set; }
        public string Line { get; set; }
        public PartiesListType PartiesList { get; set; }
        public NleArrivalType Arrival { get; set; }
        public NleBLsListType BLsList { get; set; }
    }

    // Общие классы
    public class DocHeadType
    {
        public string DocName { get; set; }
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string? Modification { get; set; }
    }

    public class PartiesListType
    {
        [XmlElement("Party")]
        public List<PartyType> Parties { get; set; }
    }

    public class PartyType
    {
        public string PartyFunction { get; set; }
        public string Name { get; set; }
        public string? ContractID { get; set; }
    }

    public class ArrivalType
    {
        public string VesselID { get; set; }
        public string Vessel { get; set; }
        public DateTime ArrivalDate { get; set; }
    }
    public class NutepArrivalType : ArrivalType
    {

    }

    public class NleArrivalType : ArrivalType
    {
        public string VoyageNoIn { get; set; }
    }

    // Классы для NUTEP
    public class BLsListType
    {
        [XmlElement("BL")]
        public List<BLType> BLs { get; set; }
    }

    public class BLType
    {
        public BLDocHeadType DocHead { get; set; }
        public BLPartiesListType PartiesList { get; set; }
        public string OceanLineID { get; set; }
        public string OriginID { get; set; }
        public ContainerListType ContainerList { get; set; }
    }

    public class BLDocHeadType
    {
        public string DocName { get; set; }
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string? Modification { get; set; }
        public string? HouseNumber { get; set; }
        public string? Carrier { get; set; }
    }

    public class BLPartiesListType
    {
        [XmlElement("Party")]
        public List<BLPartyType> Parties { get; set; }
    }

    public class BLPartyType
    {
        public string PartyFunction { get; set; }
        public string Name { get; set; }
    }

    public class ContainerListType
    {
        [XmlElement("Container")]
        public List<ContainerType> Containers { get; set; }
    }

    public class ContainerType
    {
        public string Prefix { get; set; }
        public string Number { get; set; }
        public string ISOType { get; set; }
        public bool IsEmpty { get; set; }
        public int TareWeight { get; set; }
        public string CargoName { get; set; }
        public string CargoPlaces { get; set; }
        public string CargoWeight { get; set; }
        public SealListType SealList { get; set; }
        public TemperatureConditionType? TemperatureCondition { get; set; }

        public IMOListType IMOList { get; set; }
        public string UNHazardCodes { get; set; }
    }

    [XmlRoot("IMOList")]
    public class IMOListType
    {
        [XmlElement("IMO")]
        public List<string> IMOCodes { get; set; } = new List<string>();
    }

    // Классы для NLE
    public class NleBLsListType
    {
        [XmlElement("BL")]
        public List<NleBLType> BLs { get; set; }
    }

    public class NleBLType
    {
        public BLDocHeadType DocHead { get; set; }
        public NleBLPartiesListType PartiesList { get; set; }
        public string IsDirect { get; set; }
        public string OriginID { get; set; }
        public NleContainerListType ContainerList { get; set; }
    }

    public class NleBLPartiesListType
    {
        [XmlElement("Party")]
        public List<NleBLPartyType> Parties { get; set; }
    }

    public class NleBLPartyType
    {
        public string PartyFunction { get; set; }
        public string Name { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
    }

    public class NleContainerListType
    {
        [XmlElement("Container")]
        public List<NleContainerType> Containers { get; set; }
    }

    public class NleContainerType
    {
        public string Prefix { get; set; }
        public string Number { get; set; }
        public string ISOType { get; set; }
        public bool IsEmpty { get; set; }
        public string TareWeight { get; set; }
        public string CargoName { get; set; }
        public string oversized { get; set; }
        public string NumberOfUnits { get; set; }
        public string CargoWeight { get; set; }
        public SealListType SealList { get; set; }
        public string CustomKindPlanOut { get; set; }
        public TemperatureConditionType? TemperatureCondition { get; set; }

        public IMOListType IMOList { get; set; }
        public string UNHazardCodes { get; set; }
        public string instruction { get; set; }
    }

    // Общие вспомогательные классы
    public class SealListType
    {
        [XmlElement("Seal")]
        public List<string> Seals { get; set; }
    }

    public class TemperatureConditionType
    {
        public string Value { get; set; }
        public string Unit { get; set; }
    }
}