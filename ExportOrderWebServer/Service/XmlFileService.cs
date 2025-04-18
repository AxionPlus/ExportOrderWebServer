using System.Globalization;
using System.Xml;

namespace ExportOrderWebServer.Service;

public interface IXmlFileService : IDisposable
{
    Task<byte[]> CreateXMLfile(ExportOrderDTO item);
}

public class XmlFileService : IXmlFileService
{
    private string TemporaryFilePath { get; set; } = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles", $"{Path.GetRandomFileName()}.xml"
    );

    public async Task<byte[]> CreateXMLfile(ExportOrderDTO item)
    { 
        if (string.IsNullOrEmpty(TemporaryFilePath)) return Array.Empty<byte>();

        var Commodities = item.ExportOrderRecordsDTO.GroupBy(r => new { r.DocumentName, r.SeqContent })
                                                    .Select(g => new
                                                    {
                                                        g.Key.DocumentName,
                                                        g.Key.SeqContent,
                                                        Commodity = g.Select(eor => eor.Commodity).FirstOrDefault(),
                                                        HScode = g.Select(eor => eor.HSCode).FirstOrDefault(),
                                                        CommodityGrossWt = g.Sum(eor => eor.GrossWt),
                                                        CommodityNetWt = g.Sum(eor => eor.NetWt),
                                                        AddUnitCode = g.Select(eor => eor.AdditionalUnitCode).FirstOrDefault(),
                                                        AddUnitName = g.Select(eor => eor.AdditionalUnitName).FirstOrDefault(),
                                                        AddUnitQuantity = g.Select(eor => eor.AdditionalUnitQuantity).FirstOrDefault(),
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
                xml.WriteElementString("DocumentDate", reverseDateStringXml(item.xmlDated!));
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
                    xml.WriteElementString("MeasureUnitQualifierCode", string.IsNullOrWhiteSpace(commodity.AddUnitCode) ? string.Empty : commodity.AddUnitCode);
                    xml.WriteElementString("MeasureUnitQualifierName", string.IsNullOrWhiteSpace(commodity.AddUnitName) ? string.Empty : commodity.AddUnitName);
                    xml.WriteElementString("SupplementaryGoodsQuantity", commodity.AddUnitQuantity.HasValue ? commodity.AddUnitQuantity.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")) : "");
                    //xml.WriteElementString("WarehouseName", item.TerminalName);

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

    public void Dispose()
    {
        if (File.Exists(TemporaryFilePath))
            File.Delete(TemporaryFilePath);
    }

    private readonly Func<string, string> reverseDateStringXml = (inDate) =>
    {
        string date = inDate[..10];  // inDate.Substring(0, 10);
        string time = inDate[11..];     // inDate.Substring(11)

        return date.Split('.')[2] + "-" +
                date.Split('.')[1] + "-" +
                date.Split('.')[0] + "T" + time;
    };
}
