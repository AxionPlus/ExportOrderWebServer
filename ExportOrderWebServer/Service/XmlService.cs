using System.Globalization;
using System.Xml;

namespace ExportOrderWebServer.Service;

public class XmlService : IDisposable
{
    private string? FilePath { get; set; }
    private readonly ExportOrderDTO Item;

    public XmlService(string? filePath, ExportOrderDTO item)
    {
        FilePath = filePath;
        Item = item;
    }

    public async Task<byte[]> CreateXMLfile()
    { 
        if (string.IsNullOrEmpty(FilePath)) return Array.Empty<byte>();

        var Commodities = Item.exportOrderRecordsDTO.GroupBy(r => new { r.DocumentName, r.SeqContent })
                                        .Select(g => new
                                        {
                                            g.Key.DocumentName,
                                            g.Key.SeqContent,
                                            Commodity = g.Select(g => g.Commodity).FirstOrDefault(),
                                            HScode = g.Select(g => g.HSCode).FirstOrDefault(),
                                            CommodityGrossWt = g.Sum(g => g.GrossWt),
                                            CommodityNetWt = g.Sum(g => g.NetWt),
                                            Cntrs = g.Select(g => g.Cntr).Distinct().ToList(),
                                        }).OrderBy(g => g.DocumentName).ToList();

        try
        {
            using (XmlTextWriter xml = new XmlTextWriter(FilePath, Encoding.UTF8))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();

                xml.WriteStartElement("COMMISSIONSHIPMENT");
                //xml.WriteAttributeString("xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "ReleaseOrder.xsd");
                xml.WriteAttributeString("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                xml.WriteStartElement("COMMISSIONSHIPMENT_ITEM");
                xml.WriteElementString("BorderCustomCode", Item.CustomsOfficeCode);
                xml.WriteElementString("BorderCustomsOfficeName", Item.CustomsOfficeNameShort);
                xml.WriteElementString("DocumentNumber", Item.Num);
                xml.WriteElementString("DocumentDate", reverseDateStringXml(Item.xmlDated!));
                xml.WriteElementString("GoodsDescription", string.Empty);
                xml.WriteElementString("TotalPlacesQuantity", Item.exportOrderRecordsDTO.Sum(r => r.PackageQty).ToString());
                xml.WriteElementString("TotalVolumeQuantity", Item.exportOrderRecordsDTO.Sum(r => r.PackageQty).ToString());
                xml.WriteElementString("TotalGrossWeightQuantity", Item.TotalGrossWeight!.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")));
                xml.WriteElementString("TotalNetWeightQuantity", Item.TotalNetWeight!.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")));
                xml.WriteElementString("Carrier_Name", Item.CarrierNameEn);
                xml.WriteElementString("Carrier_CountryName", string.Empty);
                xml.WriteElementString("Consignee_Name", Item.Consignees);
                xml.WriteElementString("Consignee_CountryName", string.Empty);
                xml.WriteElementString("Consignor_Name", Item.Shippers);
                xml.WriteElementString("Consignor_CountryName", string.Empty);
                xml.WriteElementString("VesselName", Item.VesselName);
                xml.WriteElementString("Vessel_CountryName", Item.VesselFlag);
                xml.WriteElementString("LoadingName", Item.POL);
                xml.WriteElementString("LoadingCode", Item.TerminalName);
                xml.WriteElementString("UnloadingName", Item.PODEn);
                xml.WriteElementString("UnloadingCode", string.Empty);
                xml.WriteElementString("DocSig_PersonName", Item.PersonXml);

                xml.WriteStartElement("COMMISSIONSHIPMENTGoods");

                int counterDocuments = 0;
                string document = string.Empty;

                foreach (var commodity in Commodities!)
                {
                    if (!document.Equals(commodity.DocumentName))
                        ++counterDocuments;

                    document = commodity.DocumentName;

                    xml.WriteStartElement("COMMISSIONSHIPMENTGOODS_ITEM");
                    xml.WriteElementString("GoodsNumericDT", commodity.SeqContent.ToString());
                    xml.WriteElementString("GoodsNumeric", counterDocuments.ToString());

                    xml.WriteElementString("GoodsCode", commodity.HScode);
                    xml.WriteElementString("GTDID", commodity.DocumentName);
                    xml.WriteElementString("GoodsDescription", commodity.Commodity);
                    xml.WriteElementString("GrossWeightQuantity", commodity.CommodityGrossWt == 0 ? "0" : commodity.CommodityGrossWt!.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")));
                    xml.WriteElementString("NetWeightQuantity", commodity.CommodityNetWt == 0 ? "0" : commodity.CommodityNetWt!.Value.ToString("########0.###", CultureInfo.GetCultureInfo("en-US")));
                    xml.WriteElementString("WarehouseName", Item.TerminalName);

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

            byte[] fileBytes = File.ReadAllBytes(FilePath);
            
            await Task.Delay(200);

            return fileBytes;
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return Array.Empty<byte>();
        }        
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }

    #region FUNCTIONS

    Func<string, string> reverseDateStringXml = (inDate) =>
    {
        string date = inDate.Substring(0, 10);
        string time = inDate.Substring(11);

        return date.Split('.')[2] + "-" +
                date.Split('.')[1] + "-" +
                date.Split('.')[0] + "T" + time;                
    };

    #endregion
}
