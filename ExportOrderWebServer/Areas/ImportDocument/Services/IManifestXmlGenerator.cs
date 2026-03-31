using System.Xml;
using System.Xml.Serialization;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.Services
{
    public interface IManifestXmlGenerator
    {
        byte[] GenerateManifestXml(VesselCallDto vesselCall);
        string GenerateManifestXmlString(VesselCallDto vesselCall);
    }

    public class ManifestXmlGenerator : IManifestXmlGenerator
    {
        public byte[] GenerateManifestXml(VesselCallDto vesselCall)
        {
            var manifest = CreateManifest(vesselCall);
            return SerializeToXmlBytes(manifest);
        }

        public string GenerateManifestXmlString(VesselCallDto vesselCall)
        {
            var manifest = CreateManifest(vesselCall);
            return SerializeToXmlString(manifest);
        }

        private ManifestModel CreateManifest(VesselCallDto vesselCall)
        {
            return new ManifestModel
            {
                DocHead = new DocHead
                {
                    DocName = "Manifest",
                    DocNumber = GenerateManifestNumber(),
                    DocDate = DateTime.Now,
                    Modification = "create"
                },
                Line = "HUBSHIPPING", // Можно вынести в конфигурацию
                Carrier = "ALPHA", // Можно взять из данных
                PartiesList = new PartiesList
                {
                    Parties = new List<Party>
                    {
                        new Party
                        {
                            PartyFunction = "Agent",
                            Name = "HUBSHIPPING",
                            ContractID = "597337957"
                        },
                        new Party
                        {
                            PartyFunction = "Stevedore",
                            Name = vesselCall.Terminal?.Name ?? "НУТЭП"
                        }
                    }
                },
                Arrival = new Arrival
                {
                    VesselID = "0000000000", // Можно генерировать или брать из VesselDto
                    Vessel = vesselCall.Vessel?.Name ?? string.Empty,
                    ArrivalDate = vesselCall.ETA
                },
                BLsList = new BLsList
                {
                    BLs = CreateBlList(vesselCall.BillOfLadings)
                }
            };
        }

        private List<BL> CreateBlList(List<BillOfLadingBaseDto> billOfLadings)
        {
            var bls = new List<BL>();

            foreach (var bl in billOfLadings)
            {
                var blModel = new BL
                {
                    DocHead = new DocHead
                    {
                        DocName = "Коносамент",
                        DocNumber = bl.Num,
                        DocDate = bl.TsDate,
                        Modification = "create",
                        HouseNumber = "VUXXDAMNOV25018N", // Можно взять из данных
                        Carrier = bl.Carrier,
                    },
                    PartiesList = new PartiesList
                    {
                        Parties = new List<Party>
                        {
                            new Party
                            {
                                PartyFunction = "Shipper",
                                Name = bl.ShipperNameRu ?? bl.ShipperName ?? bl.Shipper
                            },
                            new Party
                            {
                                PartyFunction = "Consignee",
                                Name = bl.ConsigneeNameRu ?? bl.ConsigneeName ?? bl.Consignee
                            },
                            new Party
                            {
                                PartyFunction = "Notify",
                                Name = bl.ConsigneeNameRu ?? bl.ConsigneeName ?? bl.Consignee
                            }
                        }
                    },
                    OceanLineID = "597342273", // Можно взять из данных
                    OriginID = "545080787", // Можно взять из данных
                    ContainerList = new ContainerList
                    {
                        Containers = CreateContainers(bl.ContainerRecords)
                    }
                };

                bls.Add(blModel);
            }

            return bls;
        }

        private List<Container> CreateContainers(List<BillOfLadingContainerRecordBaseDto> containerRecords)
        {
            var containers = new List<Container>();

            foreach (var containerRecord in containerRecords.OrderBy(s=>s.ContainerNo))
            {
                var container = new Container
                {
                    Prefix = ExtractContainerPrefix(containerRecord.ContainerNo),
                    Number = ExtractContainerNumber(containerRecord.ContainerNo),
                    ISOType = containerRecord.IsoCode,
                    IsEmpty = containerRecord.FullOrEmpty?.ToLower() == "empty",
                    TareWeight = FormatWeight(containerRecord.GrossWeight),
                    CargoName = containerRecord.CargoDescriptionRu ?? containerRecord.CargoDescriptionRu ?? string.Empty,
                    CargoPlaces = containerRecord.NoOfPackage,
                    CargoWeight = FormatWeight(containerRecord.GrossWeight),
                    SealList = new SealList
                    {
                        Seals = new List<string> { containerRecord.SealNo }
                    }
                };

                // Добавляем температурные условия для рефрижераторов
                if (!string.IsNullOrEmpty(containerRecord.ReeferTemp))
                {
                    container.TemperatureCondition = new TemperatureCondition
                    {
                        Value = containerRecord.ReeferTemp,
                        Unit = containerRecord.ReeferTempUOM ?? "C"
                    };
                }

                // Добавляем опасные грузы
                if (!string.IsNullOrEmpty(containerRecord.IMCOClass))
                {
                    container.DangerClasses = new DangerClasses
                    {
                        DangerClassesList = new List<DangerClass>
                        {
                            new DangerClass
                            {
                                IMO_class = containerRecord.IMCOClass,
                                UN = containerRecord.IMCONumber ?? string.Empty
                            }
                        }
                    };
                }

                containers.Add(container);
            }

            return containers;
        }

        private string ExtractContainerPrefix(string containerNo)
        {
            if (string.IsNullOrWhiteSpace(containerNo) || containerNo.Length < 4)
                return string.Empty;

            return containerNo.Substring(0, 4);
        }

        private string ExtractContainerNumber(string containerNo)
        {
            if (string.IsNullOrWhiteSpace(containerNo) || containerNo.Length <= 4)
                return string.Empty;

            return containerNo.Substring(4);
        }

        private string FormatWeight(double weight)
        {
            return weight.ToString("F3").Replace(".", ",");
        }

        private string GenerateManifestNumber()
        {
            // Генерация номера манифеста, можно использовать текущую дату + инкремент
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private byte[] SerializeToXmlBytes(ManifestModel manifestModel)
        {
            var serializer = new XmlSerializer(typeof(ManifestModel));
            using var memoryStream = new MemoryStream();

            var settings = new XmlWriterSettings
            {
                Encoding = System.Text.Encoding.UTF8,
                Indent = true
            };

            using var writer = XmlWriter.Create(memoryStream, settings);

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            serializer.Serialize(writer, manifestModel, namespaces);

            return memoryStream.ToArray();
        }

        private string SerializeToXmlString(ManifestModel manifestModel)
        {
            var serializer = new XmlSerializer(typeof(ManifestModel));
            using var writer = new StringWriter();

            var settings = new XmlWriterSettings
            {
                Encoding = System.Text.Encoding.UTF8,
                Indent = true
            };

            using var xmlWriter = XmlWriter.Create(writer, settings);

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            serializer.Serialize(xmlWriter, manifestModel, namespaces);

            return writer.ToString();
        }
    }

}