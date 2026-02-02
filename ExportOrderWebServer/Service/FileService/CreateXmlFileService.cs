using System.Globalization;
using System.Xml;

namespace ExportOrderWebServer.Service.FileService;

public interface ICreateXmlFileService : IDisposable
{
    Task<byte[]> CreateXmlFileExportOrder(ExportOrderFileDto item);
}

public class CreateXmlFileService : ICreateXmlFileService
{
    private readonly static string DirTemporary = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TempFiles");
    private readonly static string TemporaryFilePath = Path.Combine(DirTemporary, $"{Path.GetRandomFileName()}.xml");

    public CreateXmlFileService()
    {
        if (!Directory.Exists(DirTemporary))
            Directory.CreateDirectory(DirTemporary);
    }

    public async Task<byte[]> CreateXmlFileExportOrder(ExportOrderFileDto item)
    {
        var numberFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone(); // - не используется здесь
        numberFormat.NumberGroupSeparator = ","; // запятая как разделитель групп разрядов
        // CultureInfo.InvariantCulture - всегда использует точку как разделитель дробной части и не добавляет разделители групп разрядов.
        // "F" - фиксированный формат
        // "N" - числовой формат (может добавлять разделители групп)

        var Commodities = item.Records.GroupBy(r => new { r.DocumentName, r.SeqContent })
                                                    .Select(g => new
                                                    {
                                                        g.Key.DocumentName,
                                                        g.Key.SeqContent,
                                                        Commodity = g.Select(eor => eor.Commodity).FirstOrDefault(),
                                                        HScode = g.Select(eor => eor.HSCode).FirstOrDefault(),
                                                        CommodityGrossWts = g.Sum(eor => eor.GrossWt),
                                                        CommodityNetWts = g.Sum(eor => eor.NetWt),
                                                        SupplementaryUnitQuantity = g.Select(eor => eor.SupplementaryUnitQuantity).FirstOrDefault(),
                                                        SupplementaryUnitCode = g.Select(eor => eor.SupplementaryUnitCode).FirstOrDefault(),
                                                        SupplementaryUnitShortName = g.Select(eor => eor.SupplementaryUnitShortName).FirstOrDefault(),
                                                        ContainerNums = g.Select(eor => eor.ContainerNum).Distinct().ToArray(),
                                                    }).OrderBy(g => g.DocumentName).ToArray();

        if (Commodities is null || Commodities.Length == 0)
            return Array.Empty<byte>();

        try
        {
            using (XmlTextWriter xml = new(TemporaryFilePath, Encoding.UTF8))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartDocument();

                xml.WriteStartElement("COMMISSIONSHIPMENT");
                xml.WriteAttributeString("xsi", "http://www.w3.org/2001/XMLSchema-instance");

                xml.WriteStartElement("COMMISSIONSHIPMENT_ITEM");
                xml.WriteElementString("WarehouseName", item.TerminalName);
                xml.WriteElementString("BorderCustomCode", item.CustomsOfficeCode);
                xml.WriteElementString("BorderCustomsOfficeName", item.CustomsOfficeNameShort);
                xml.WriteElementString("DocumentNumber", item.Num);
                xml.WriteElementString("DocumentDate", item.Dated.HasValue ? item.Dated.Value.ToString("s", CultureInfo.InvariantCulture) : "");  // "yyyy-MM-ddTHH:mm:ss"
                xml.WriteElementString("GoodsDescription", string.Empty);
                xml.WriteElementString("TotalPlacesQuantity", item.Records.Sum(r => r.PackageQty).ToString());
                xml.WriteElementString("TotalVolumeQuantity", item.Records.Sum(r => r.PackageQty).ToString());                
                xml.WriteElementString("TotalGrossWeightQuantity", item.Records.Sum(r => r.GrossWt)?.ToString(CultureInfo.InvariantCulture));   // "N3" или .ToString("########0.###", CultureInfo.GetCultureInfo("en-US")
                xml.WriteElementString("TotalNetWeightQuantity", item.Records.Sum(r => r.NetWt)?.ToString(CultureInfo.InvariantCulture));    // "N3" или .ToString("########0.###", CultureInfo.GetCultureInfo("en-US")
                xml.WriteElementString("Carrier_Name", item.CarrierNameEn);
                xml.WriteElementString("Carrier_CountryName", string.Empty);
                xml.WriteElementString("Consignee_Name", string.Join(";\n", item.Consignees));
                xml.WriteElementString("Consignee_CountryName", string.Empty);
                xml.WriteElementString("Consignor_Name", string.Join(";\n", item.Shippers));
                xml.WriteElementString("Consignor_CountryName", string.Empty);
                xml.WriteElementString("VesselName", item.VesselName);
                xml.WriteElementString("Vessel_CountryName", item.VesselFlag);
                xml.WriteElementString("LoadingName", item.POL);
                xml.WriteElementString("LoadingCode", item.TerminalName);
                xml.WriteElementString("UnloadingName", item.PortOfDischargeEn);
                xml.WriteElementString("UnloadingCode", string.Empty);
                xml.WriteElementString("DocSig_PersonName", item.PersonXml);

                xml.WriteStartElement("COMMISSIONSHIPMENTGoods");

                int counterDocuments = 0;                
                string document = string.Empty;

                foreach (var commodity in Commodities)
                {
                    if (!document.Equals(commodity.DocumentName))
                        ++counterDocuments;

                    document = commodity.DocumentName ?? string.Empty;

                    xml.WriteStartElement("COMMISSIONSHIPMENTGOODS_ITEM");
                    xml.WriteElementString("GoodsNumericDT", commodity.SeqContent.ToString());
                    xml.WriteElementString("GoodsNumeric", counterDocuments.ToString());
                    xml.WriteElementString("GTDID", commodity.DocumentName);
                    xml.WriteElementString("GoodsCode", commodity.HScode);
                    xml.WriteElementString("GoodsDescription", commodity.Commodity);
                    xml.WriteElementString("GrossWeightQuantity", commodity.CommodityGrossWts == 0 ? "0" : commodity.CommodityGrossWts?.ToString(CultureInfo.InvariantCulture));
                    xml.WriteElementString("NetWeightQuantity", commodity.CommodityNetWts == 0 ? "0" : commodity.CommodityNetWts?.ToString(CultureInfo.InvariantCulture));

                    if (!string.IsNullOrEmpty(commodity.SupplementaryUnitCode))
                    {
                        xml.WriteElementString("MeasureUnitQualifierCode", commodity.SupplementaryUnitCode);
                        xml.WriteElementString("MeasureUnitQualifierName", commodity.SupplementaryUnitShortName ?? string.Empty);                        
                        xml.WriteElementString("SupplementaryGoodsQuantity", commodity.SupplementaryUnitQuantity?.ToString(CultureInfo.InvariantCulture));
                    }

                    xml.WriteStartElement("COMMISSIONSHIPMENTContainer");
                    foreach (var containerNum in commodity.ContainerNums)
                    {
                        xml.WriteStartElement("COMMISSIONSHIPMENTCONTAINER_ITEM");
                        xml.WriteElementString("ContainerID", containerNum);
                        xml.WriteEndElement();
                    }

                    xml.WriteEndElement();
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
                xml.WriteEndElement();
            }

            byte[] fileBytes = File.ReadAllBytes(TemporaryFilePath);

            await Task.Delay(100);

            return fileBytes;
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.Message);            
            Dispose();
            return Array.Empty<byte>();
            throw new Exception(ex.Message);            
        }
    }

    #region AXILAIRY
    private readonly Func<string?, string> reverseDateStringXml = (inDate) =>
    {
        if (string.IsNullOrWhiteSpace(inDate)) return string.Empty;

        string date = inDate[..10];
        string time = inDate[11..];

        return string.Concat(date.Split('.')[2], "-",
                                date.Split('.')[1], "-",
                                date.Split('.')[0], "T", time);
    };
    #endregion

    public void Dispose()
    {
        foreach (var file in Directory.GetFiles(DirTemporary))
        {
            File.Delete(file);
        }            
    }
}
