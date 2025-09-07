using System.Globalization;
using System.Xml;

namespace ExportOrderWebServer.Service;

public interface IXmlFileCreateService : IDisposable
{
    Task<byte[]> CreateXMLfileExportOrder(ExportOrderDTO item);
    Task<byte[]> CreateXMLfileManifestNle(ImportVesselCallDto importVesselCall);
}

public class XmlFileCreateService : IXmlFileCreateService
{
    private readonly static string DirTemporary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles");
    private string TemporaryFilePath { get; set; } = Path.Combine(DirTemporary, $"{Path.GetRandomFileName()}.xml");

    public XmlFileCreateService()
    {
        if (!Directory.Exists(DirTemporary))
            Directory.CreateDirectory(DirTemporary);
    }

    public async Task<byte[]> CreateXMLfileExportOrder(ExportOrderDTO item)
    {
        var Commodities = item.ExportOrderRecordsDTO.GroupBy(r => new { r.DocumentName, r.SeqContent })
                                                    .Select(g => new
                                                    {
                                                        g.Key.DocumentName,
                                                        g.Key.SeqContent,
                                                        Commodity = g.Select(eor => eor.Commodity).FirstOrDefault(),
                                                        HScode = g.Select(eor => eor.HSCode).FirstOrDefault(),
                                                        CommodityGrossWt = g.Sum(eor => eor.GrossWt),
                                                        CommodityNetWt = g.Sum(eor => eor.NetWt),
                                                        SupplementaryUnitQuantity = g.Select(eor => eor.SupplementaryUnitQuantity).FirstOrDefault(),
                                                        SupplementaryUnitCode = g.Select(eor => eor.SupplementaryUnitCode).FirstOrDefault(),
                                                        SupplementaryUnitShortName = g.Select(eor => eor.SupplementaryUnitShortName).FirstOrDefault(),
                                                        Cntrs = g.Select(eor => eor.Cntr).Distinct().ToList(),
                                                    }).OrderBy(g => g.DocumentName).ToList();

        if (Commodities is null || !Commodities.Any())
            return Array.Empty<byte>();

        try
        {
            using (XmlTextWriter xml = new(TemporaryFilePath, Encoding.UTF8))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();

                xml.WriteStartElement("COMMISSIONSHIPMENT");
                //xml.WriteAttributeString("xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "ReleaseOrder.xsd");
                xml.WriteAttributeString("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                xml.WriteStartElement("COMMISSIONSHIPMENT_ITEM");
                xml.WriteElementString("WarehouseName", item.TerminalName);
                xml.WriteElementString("BorderCustomCode", item.CustomsOfficeCode);
                xml.WriteElementString("BorderCustomsOfficeName", item.CustomsOfficeNameShort);
                xml.WriteElementString("DocumentNumber", item.Num);
                xml.WriteElementString("DocumentDate", reverseDateStringXml(item.XmlDated));
                xml.WriteElementString("GoodsDescription", string.Empty);
                xml.WriteElementString("TotalPlacesQuantity", item.ExportOrderRecordsDTO.Sum(r => r.PackageQty).ToString());
                xml.WriteElementString("TotalVolumeQuantity", item.ExportOrderRecordsDTO.Sum(r => r.PackageQty).ToString());
                xml.WriteElementString("TotalGrossWeightQuantity", item.TotalGrossWeight!.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")));
                xml.WriteElementString("TotalNetWeightQuantity", item.TotalNetWeight!.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")));
                xml.WriteElementString("Carrier_Name", item.CarrierNameEn);
                xml.WriteElementString("Carrier_CountryName", string.Empty);
                xml.WriteElementString("Consignee_Name", item.Consignees);
                xml.WriteElementString("Consignee_CountryName", string.Empty);
                xml.WriteElementString("Consignor_Name", item.Shippers);
                xml.WriteElementString("Consignor_CountryName", string.Empty);
                xml.WriteElementString("VesselName", item.VesselName);
                xml.WriteElementString("Vessel_CountryName", item.VesselFlag);
                xml.WriteElementString("LoadingName", item.POL);
                xml.WriteElementString("LoadingCode", item.TerminalName);
                xml.WriteElementString("UnloadingName", item.PODEn);
                xml.WriteElementString("UnloadingCode", string.Empty);
                xml.WriteElementString("DocSig_PersonName", item.PersonXml);

                xml.WriteStartElement("COMMISSIONSHIPMENTGoods");

                int counterDocuments = 0;
                string document = string.Empty;

                foreach (var commodity in Commodities)
                {
                    if (!document.Equals(commodity.DocumentName))
                        ++counterDocuments;

                    document = commodity.DocumentName;

                    xml.WriteStartElement("COMMISSIONSHIPMENTGOODS_ITEM");
                    xml.WriteElementString("GoodsNumericDT", commodity.SeqContent.ToString());
                    xml.WriteElementString("GoodsNumeric", counterDocuments.ToString());
                    xml.WriteElementString("GTDID", commodity.DocumentName);
                    xml.WriteElementString("GoodsCode", commodity.HScode);
                    xml.WriteElementString("GoodsDescription", commodity.Commodity);
                    xml.WriteElementString("GrossWeightQuantity", commodity.CommodityGrossWt == 0 ? "0" : commodity.CommodityGrossWt!.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")));
                    xml.WriteElementString("NetWeightQuantity", commodity.CommodityNetWt == 0 ? "0" : commodity.CommodityNetWt!.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")));

                    if (!string.IsNullOrEmpty(commodity.SupplementaryUnitCode))
                    {
                        xml.WriteElementString("MeasureUnitQualifierCode", commodity.SupplementaryUnitCode);
                        xml.WriteElementString("MeasureUnitQualifierName", commodity.SupplementaryUnitShortName ?? string.Empty);
                        xml.WriteElementString("SupplementaryGoodsQuantity", commodity.SupplementaryUnitQuantity.HasValue ?
                                                    commodity.SupplementaryUnitQuantity.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")) : "");
                    }

                    //xml.WriteElementString("WarehouseName", item.TerminalName); - moved to StartElement("COMMISSIONSHIPMENT_ITEM")

                    xml.WriteStartElement("COMMISSIONSHIPMENTContainer");
                    foreach (var cntr in commodity.Cntrs)
                    {
                        xml.WriteStartElement("COMMISSIONSHIPMENTCONTAINER_ITEM");
                        xml.WriteElementString("ContainerID", cntr);
                        xml.WriteEndElement();
                    }

                    xml.WriteEndElement();
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
                xml.WriteEndElement();
            }

            byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);

            await Task.Delay(200);

            return fileBytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Dispose();
            return Array.Empty<byte>();
        }
    }

    public async Task<byte[]> CreateXMLfileManifestNle(ImportVesselCallDto importVesselCall)
    {
        try
        {
            await using (XmlTextWriter xml = new(TemporaryFilePath, Encoding.GetEncoding(1251)))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();
                xml.WriteStartElement("Manifest"); // Manifest

                #region DocHead

                xml.WriteStartElement("DocHead"); // DocHead
                xml.WriteElementString("DocName", "Manifest"); //< DocName > Manifest </ DocName >
                xml.WriteElementString("DocNumber",
                    $"{importVesselCall.VesselVoyage}"); //< DocNumber > Manifest_FRITZ REUTER_2002S </ DocNumber >
                xml.WriteElementString("DocDate",
                    DocDate(DateTime.Now)); //< DocDate > 2020 - 01 - 20T00: 00:00 </ DocDate >
                xml.WriteElementString("Modification", "create"); //< Modification > create </ Modification >

                //< DeparturePort >
                //< Name > DILISKELESI </ Name >
                //< Code > TRDIL </ Code >
                //< CountryCode > TR </ CountryCode >
                //</ DeparturePort >
                xml.WriteEndElement(); // END DocHead

                #endregion

                xml.WriteElementString("Line", "SOLING_AGE"); //< Line > SOLING_AGE </ Line >

                #region PartiesList

                xml.WriteStartElement("PartiesList"); // < PartiesList >

                xml.WriteStartElement("Party"); //< Party >
                xml.WriteElementString("PartyFunction", "Agent"); //< PartyFunction > Agent </ PartyFunction >
                xml.WriteElementString("Name", ""); //< Name > EVG </ Name >
                xml.WriteElementString("ContractID",
                    "05/04/25*SOLING_AGENCY"); //<ContractID>10/04/24*HUBSHIPPING</ContractID>
                xml.WriteEndElement(); // END Party

                xml.WriteStartElement("Party"); //< Party >
                xml.WriteElementString("PartyFunction", "Stevedore"); //< PartyFunction > Stevedore </ PartyFunction >
                xml.WriteElementString("Name", "NLE"); //< Name > NLE </ Name >
                xml.WriteEndElement(); // END Party

                xml.WriteEndElement(); // END PartiesList

                #endregion

                #region Arrival

                xml.WriteStartElement("Arrival"); // Manifest
                xml.WriteElementString("VesselID", "0"); //  < VesselID > FRITZ REUTER </ VesselID >
                xml.WriteElementString("Vessel", importVesselCall.VesselName); //  < Vessel > FRITZ REUTER </ Vessel >
                xml.WriteElementString("ArrivalDate",
                    DocDate(importVesselCall.ETA)); //   < ArrivalDate > 2020 - 01 - 22T00: 00:00 </ ArrivalDate >
                xml.WriteElementString("VoyageNoIn",
                    importVesselCall.VesselVoyage); //   < ArrivalDate > 2020 - 01 - 22T00: 00:00 </ ArrivalDate >
                xml.WriteEndElement(); // END Arrival

                #endregion

                #region BLsList

                xml.WriteStartElement("BLsList"); // BLsList

                foreach (var billOfLadingDto in importVesselCall.BillofLadings.OrderBy(s => s.Num).ToList())
                {
                    xml.WriteStartElement("BL"); // BL

                    #region DocHead

                    xml.WriteStartElement("DocHead"); // DocHead
                    xml.WriteElementString("DocName", "Коносамент"); //< DocName > Коносамент </ DocName >
                    xml.WriteElementString("DocNumber",
                        $"{billOfLadingDto.Num}"); //< DocNumber > EGLV040900388675 </ DocNumber >
                    //xml.WriteElementString("DocDate", DocDate(BoL.IssueDate)); // < DocDate > 2019 - 11 - 30T00: 00:00 </ DocDate >
                    xml.WriteElementString("DocDate",
                        DocDate(billOfLadingDto.SobDate.Value)); // < DocDate > 2019 - 11 - 30T00: 00:00 </ DocDate >

                    xml.WriteEndElement(); // END DocHead

                    #endregion

                    #region PartiesList

                    xml.WriteStartElement("PartiesList"); // PartiesList

                    xml.WriteStartElement("Party"); // Party
                    xml.WriteElementString("PartyFunction", "Shipper"); //< PartyFunction > Shipper </ PartyFunction >
                    xml.WriteElementString("Name",
                        $"{billOfLadingDto.ShipperName?.Replace("&", "AND").ToUpper()}"); // < Name > INDIVIDUAL ENTREPRENEUR(IP) ENGENOV GERON GEORGIEVICH, ESSENTUKI </ Name >
                    xml.WriteElementString("Country",
                        $"{billOfLadingDto.ShipperCountryRu?.ToUpper()}"); //< Country > россия </ Country >
                    xml.WriteElementString("Address",
                        $"{billOfLadingDto.ShipperAddress?.ToUpper()}"); //<Address>Germany, Hamburg, ROEDINGSMARKT 9  D 20459 HAMBURG</Address>
                    xml.WriteEndElement(); // END Party

                    xml.WriteStartElement("Party"); // Party
                    xml.WriteElementString("PartyFunction",
                        "Consignee"); //< PartyFunction > Consignee </ PartyFunction >
                    xml.WriteElementString("Name",
                        $"{billOfLadingDto.ConsigneeNameRu?.ToUpper()}"); //< Name > KCP HEAVY INDUSTRIES CO., LTD, Корея </ Name >
                    xml.WriteElementString("Address",
                        $"{billOfLadingDto.ConsigneeAddressRu?.ToUpper()}"); //<Address>Germany, Hamburg, ROEDINGSMARKT 9  D 20459 HAMBURG</Address>
                    xml.WriteElementString("Country",
                        $"{billOfLadingDto.ConsigneeCountryRu?.ToUpper()}"); //< Country > россия </ Country >
                    xml.WriteEndElement(); // END Party

                    //xml.WriteStartElement("Party"); // Party
                    //xml.WriteElementString("PartyFunction", "Notify"); //< PartyFunction > Notify </ PartyFunction >
                    //xml.WriteElementString("Name", $"{BoL.CneeRu.Trim()}"); //< Name > KCP HEAVY INDUSTRIES CO., LTD, Корея </ Name >
                    //xml.WriteEndElement();// END Party

                    xml.WriteEndElement(); // END PartiesList

                    #endregion

                    //  xml.WriteElementString("IsDirect", "0"); // <IsDirect>0</IsDirect>
                    //xml.WriteElementString("OceanLineID", OceanLineID); // < OceanLineID > 551618559 </ OceanLineID > ---Договор с НУТЭП
                    // xml.WriteElementString("OriginID", $"{importVesselCall.POL.NameEn}"); //<OriginID>Gdansk</OriginID>
                    xml.WriteElementString("OriginID", "0"); //<OriginID>Gdansk</OriginID>

                    xml.WriteStartElement("DepartureGoodsPort");
                    xml.WriteElementString("Name", $"{importVesselCall.DeparturePortName}");
                    xml.WriteElementString("Code", $"{importVesselCall.DeparturePortCode}");
                    xml.WriteElementString("CountryCode", $"{importVesselCall.DeparturePortCountryCode}");
                    xml.WriteEndElement();

                    xml.WriteStartElement("DebarkationPort"); // Container
                    xml.WriteElementString("Name", "NOVOROSSIYSK");
                    xml.WriteElementString("Code", "RUNVS");
                    xml.WriteElementString("CountryCode", "RU");
                    xml.WriteEndElement();
                    //< DebarkationPort >
                    //    < Name > NOVOROSSIYSK </ Name >
                    //    < Code > RUNVS </ Code >
                    //    < CountryCode > RU </ CountryCode >
                    //    </ DebarkationPort >

                    #region ContainerList

                    xml.WriteStartElement("ContainerList"); // ContainerList
                    foreach (var containerRecord in billOfLadingDto.ContainerRecords)
                    {
                        xml.WriteStartElement("Container"); // Container

                        xml.WriteElementString("Prefix",
                            containerRecord.ContainerNum.Substring(0, 4)); // < Prefix > TCLU </ Prefix >
                        xml.WriteElementString("Number",
                            containerRecord.ContainerNum.Substring(4, 7)); // < Number > 4376329 </ Number >
                        xml.WriteElementString("ISOType", containerRecord.ContainerType); // <ISOType>4310</ISOType>
                        xml.WriteElementString("IsEmpty",
                            (containerRecord.IsEmpty).ToString()); // < IsEmpty > false </ IsEmpty >
                        xml.WriteElementString("TareWeight",
                            containerRecord.TareWeight.ToString()); //  < TareWeight > 3650 </ TareWeight >

                        if (!containerRecord.IsEmpty)
                        {
                            xml.WriteElementString("CargoName",
                                containerRecord.GoodsDescriptionRu
                                    ?.Trim()); // < CargoName > запасные части к бетононасосу </ CargoName >
                            //  xml.WriteElementString("oversized", "false");  // < oversized > true </ oversized >
                            xml.WriteElementString("NumberOfUnits",
                                containerRecord.PackageQty.ToString()); // < oversized > true </ oversized >
                            xml.WriteElementString("CargoWeight",
                                containerRecord.CargoWeight.ToString()
                                    ?.Replace(',', '.')); // < CargoWeight > 1000.000 </ CargoWeight >
                        }

                        //< excess_LGTH > 0 </ excess_LGTH >
                        //< excess_WDTH > 0 </ excess_WDTH >
                        //< excess_HGHT > 30 </ excess_HGHT >

                        //xml.WriteElementString("CargoPlaces", record.PkgQty_xml != null ? record.PkgQty_xml : record.PkgQty_csv); // < CargoPlaces > 1 </ CargoPlaces >

                        xml.WriteStartElement("SealList"); // SealList
                        if (!containerRecord.IsEmpty)
                        {
                            if (!string.IsNullOrWhiteSpace(containerRecord.SealNo))
                                xml.WriteElementString("Seal",
                                    containerRecord.SealNo.Trim()); //< Seal > EMCCXG4379 </ Seal >
                            if (!string.IsNullOrWhiteSpace(containerRecord.SealOth))
                                xml.WriteElementString("Seal",
                                    containerRecord.SealOth.Trim()); //< Seal > EMCCXG4379 </ Seal >
                            if (!string.IsNullOrWhiteSpace(containerRecord.SealShr))
                                xml.WriteElementString("Seal",
                                    containerRecord.SealShr.Trim()); //< Seal > EMCCXG4379 </ Seal >

                        }
                        else
                        {
                            xml.WriteElementString("Seal", string.Empty); //< Seal > EMCCXG4379 </ Seal >
                        }

                        xml.WriteEndElement(); // END SealList

                        if (!string.IsNullOrWhiteSpace(containerRecord.TempSet.ToString()))
                        {
                            xml.WriteStartElement("TemperatureCondition"); // < TemperatureCondition >
                            xml.WriteElementString("Value",
                                containerRecord.TempSet.ToString()); //< Value > -25 </ Value >
                            xml.WriteElementString("Unit", "C"); //< Unit > C </ Unit >
                            xml.WriteEndElement(); // END TemperatureCondition
                        }

                        if (containerRecord.IsImo)
                        {
                            xml.WriteStartElement("IMOList"); // < IMOList >
                            xml.WriteElementString("IMO", containerRecord.ImoClass); //<IMO>  Text </IMO>
                            xml.WriteEndElement(); // END IMOList

                            xml.WriteElementString("UNHazardCodes", containerRecord.Unno);
                        }

                        xml.WriteEndElement(); // END Container
                    }

                    xml.WriteEndElement(); // END ContainerList

                    #endregion

                    xml.WriteEndElement(); // END BL
                }

                xml.WriteEndElement(); // END BLsList

                #endregion

                xml.WriteEndElement(); // END Manifest
                xml.WriteEndDocument();

            }
            byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);

            return fileBytes;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Dispose();
            return Array.Empty<byte>();
        }
    }

    private readonly Func<string?, string> reverseDateStringXml = (inDate) =>
    {
        if (string.IsNullOrWhiteSpace(inDate)) return string.Empty;

        string date = inDate[..10];  // inDate.Substring(0, 10);
        string time = inDate[11..];  // inDate.Substring(11)

        return string.Concat(date.Split('.')[2], "-",
                                date.Split('.')[1], "-",
                                date.Split('.')[0], "T", time);
    };

    private static string DocDate(DateTime dateTime) => $"{dateTime.Year}-{dateTime:MM}-{dateTime:dd}T00:00:00";//2019 - 11 - 30T00: 00:00

    public void Dispose()
    {
        if (File.Exists(TemporaryFilePath))
            File.Delete(TemporaryFilePath);
    }
}
