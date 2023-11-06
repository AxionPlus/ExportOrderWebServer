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

        string WeightFormat = "########.000";

        string Shippers = string.Join("; КОНТРАГЕНТ; ", Item.exportOrderRecordsDTO?.Select(r => r.Shipper).Distinct().ToList()!);
        string Consignees = string.Join("; КОНТРАГЕНТ; ", Item.exportOrderRecordsDTO?.Select(r => r.ConsigneeEn).Distinct().ToList()!);

        var dsCommodities = Item.exportOrderRecordsDTO!.GroupBy(r => new { r.DocumentName, r.CommodityName } )
                                                        .Select(g => new
                                                        {
                                                            g.Key.DocumentName,
                                                            HScode = g.Select(g => g.HSCode).FirstOrDefault(),
                                                            g.Key.CommodityName,
                                                            CommodityGrossWt = g.Where(c => c.DocumentName == g.Key.DocumentName).Sum(g => g.GrossWt),
                                                            CommodityNetWt = g.Where(c => c.DocumentName == g.Key.DocumentName).Sum(g => g.NetWt),
                                                            Cntrs = g.Where(c => c.DocumentName == g.Key.DocumentName).Select(g => g.Cntr).ToList(),
                                                        }).ToList();

        try
        {
            using (XmlTextWriter xml = new XmlTextWriter(FilePath, Encoding.UTF8))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();

                xml.WriteStartElement("COMMISSIONSHIPMENT");
                //xml.WriteAttributeString("xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "ReleaseOrder.xsd");
                xml.WriteAttributeString("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                //xml.WriteStartAttribute("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                xml.WriteStartElement("COMMISSIONSHIPMENT_ITEM");
                xml.WriteElementString("BorderCustomCode", Item.CustomsOfficeCode);
                xml.WriteElementString("BorderCustomsOfficeName", Item.CustomsOfficeName);
                xml.WriteElementString("DocumentNumber", Item.Num);
                xml.WriteElementString("DocumentDate", reverseDateStringXml(Item.xmlDated));
                xml.WriteElementString("GoodsDescription", string.Empty);
                xml.WriteElementString("TotalPlacesQuantity", Item.exportOrderRecordsDTO!.Sum(r => r.PackageQty).ToString());
                xml.WriteElementString("TotalVolumeQuantity", Item.exportOrderRecordsDTO!.Sum(r => r.PackageQty).ToString());
                xml.WriteElementString("TotalGrossWeightQuantity", Item.TotalGrossWeight!.Value.ToString(WeightFormat));
                xml.WriteElementString("TotalNetWeightQuantity", Item.TotalNetWeight!.Value.ToString(WeightFormat));
                xml.WriteElementString("Carrier_Name", Item.CarrierNameEn);
                xml.WriteElementString("Carrier_CountryName", string.Empty);
                xml.WriteElementString("Consignee_Name", Consignees);
                xml.WriteElementString("Consignee_CountryName", string.Empty);
                xml.WriteElementString("Consignor_Name", Shippers);
                xml.WriteElementString("Consignor_CountryName", string.Empty);
                xml.WriteElementString("VesselName", Item.VesselName);
                xml.WriteElementString("Vessel_CountryName", Item.VesselFlag);
                xml.WriteElementString("LoadingName", Item.POL);
                xml.WriteElementString("LoadingCode", Item.TerminalName);
                xml.WriteElementString("UnloadingName", Item.PODwithCountryRus);
                xml.WriteElementString("UnloadingCode", string.Empty);
                xml.WriteElementString("DocSig_PersonName", Item.PersonXml);

                xml.WriteStartElement("COMMISSIONSHIPMENTGoods");

                int counterGoods = 0;

                foreach (var commodity in dsCommodities!)
                {
                    xml.WriteStartElement("COMMISSIONSHIPMENTGOODS_ITEM");
                    xml.WriteElementString("GoodsNumericDT", "1");
                    xml.WriteElementString("GoodsNumeric", (++counterGoods).ToString());
                    xml.WriteElementString("GoodsCode", commodity.HScode);
                    xml.WriteElementString("GTDID", commodity.DocumentName);
                    xml.WriteElementString("GoodsDescription", commodity.CommodityName);
                    xml.WriteElementString("GrossWeightQuantity", commodity.CommodityGrossWt!.Value.ToString(WeightFormat));
                    xml.WriteElementString("NetWeightQuantity", commodity.CommodityNetWt!.Value.ToString(WeightFormat));
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
