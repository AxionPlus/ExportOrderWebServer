using System.Xml;

namespace ExportOrderWebServer.Service;

public interface IXmlFileReadService : IDisposable
{
    public Task<List<ReadXmlDocumentRecordDTO>?> ReadXmlFileDocumentRecords(string filePath);
}

public class XmlFileReadService : IXmlFileReadService
{
    private string? FilePath { get; set; }

    public XmlFileReadService() { }

    public async Task<List<ReadXmlDocumentRecordDTO>?> ReadXmlFileDocumentRecords(string filePath)
    {
        FilePath = filePath;
        
        if (!File.Exists(FilePath))
        {
            return null;
            throw new FileNotFoundException(FilePath);
        }            

        XmlDocument doc = new();    //XDocument xDoc = XDocument.Load(FilePath);
        doc.Load(FilePath);         //doc.LoadXml(file);

        if (doc.DocumentElement == null || doc.DocumentElement.Name != "gdgdc:GoodsDeclaration")  //if (doc.DocumentElement == null || !doc.DocumentElement.Name.Contains("GoodsDeclaration"))
            return null;
        
        List<ReadXmlDocumentRecordDTO> readDTOs = new();

        /// DOCUMENT NAME
        string readDocumentName = string.Empty;

        foreach (XmlNode node in doc.ChildNodes)
            if (node.NodeType == XmlNodeType.Comment)
                if (node.InnerText.Contains("NOM_REG:"))
                    readDocumentName = node.InnerText.Replace("NOM_REG:", "").Trim();

        /// RECORDS
        foreach (XmlNode childNode in doc.DocumentElement.ChildNodes)
        {
            switch (childNode.Name)
            {
                case "cacdo:GDGoodsShipmentDetails":
                    foreach (XmlNode shipmentDetails in childNode.ChildNodes)
                    {
                        switch (shipmentDetails.Name)
                        {
                            case "cacdo:GDGoodsItemDetails":
                                ReadXmlDocumentRecordDTO dto = new() { DocumentName = readDocumentName };

                                foreach (XmlNode goodsItemDetails in shipmentDetails.ChildNodes)
                                {
                                    switch (goodsItemDetails.Name)
                                    {
                                        case "casdo:ConsignmentItemOrdinal":                                            
                                            dto.Seq = int.TryParse(goodsItemDetails.InnerText, out int _seq) ? _seq : 0; break;
                                        case "csdo:CommodityCode":
                                            dto.CommodityHSCode = goodsItemDetails.InnerText; break;
                                        case "casdo:GoodsDescriptionText":
                                            dto.CommodityName = goodsItemDetails.InnerText; break;
                                        case "csdo:UnifiedGrossMassMeasure":
                                            dto.GrossWt = goodsItemDetails.InnerText; break;
                                        case "csdo:UnifiedNetMassMeasure":
                                            dto.NetWt = goodsItemDetails.InnerText; break;
                                    }
                                }
                                readDTOs.Add(dto);
                            break;
                        }
                    }
                break;
            }
        }

        await Task.Delay(10);

        return readDTOs;
    }

    public void Dispose()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }
}
