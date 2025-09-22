using System.Xml;

namespace ExportOrderWebServer.Service;

public interface IXmlFileReadService : IDisposable
{
    public Task<List<ReadXmlDocumentRecordDTO>?> ReadXmlFileDocument(string filePath);
}

public class XmlFileReadService : IXmlFileReadService
{
    private string? FilePath { get; set; }

    public XmlFileReadService() { }

    public async Task<List<ReadXmlDocumentRecordDTO>?> ReadXmlFileDocument(string filePath)
    {
        FilePath = filePath;
        
        if (!File.Exists(FilePath))
        {
            return null;
            throw new FileNotFoundException(FilePath);
        }            

        XmlDocument doc = new();    //XDocument xDoc = XDocument.Load(FilePath);
        doc.Load(FilePath);         //doc.LoadXml(file);

        if (doc.DocumentElement == null)
            return null;

        List<ReadXmlDocumentRecordDTO> readDTOs = new();

        /// DOCUMENT NAME
        string readDocumentName = string.Empty;

        try
        {
            #region BELARUS

            if (doc.DocumentElement.Name == "gdgdc:GoodsDeclaration")
            {
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
            }

            #endregion

            #region RUSSIA

            if (doc.DocumentElement.Name == "ED_Container")
            {
                foreach (XmlNode node in doc.ChildNodes)
                    if (node.NodeType == XmlNodeType.Comment)
                        if (node.InnerText.Contains("ND="))
                            readDocumentName = node.InnerText.Replace("ND=", "").Trim();

                /// RECORDS
                foreach (XmlNode childNode in doc.DocumentElement.ChildNodes)
                {
                    switch (childNode.Name)
                    {
                        case "ContainerDoc":
                            foreach (XmlNode containerDocDetails in childNode.ChildNodes)
                            {
                                switch (containerDocDetails.Name)
                                {
                                    case "DocBody":
                                        foreach (XmlNode bodyDetails in containerDocDetails.ChildNodes)
                                        {
                                            switch (bodyDetails.Name)
                                            {
                                                case "ESADout_CU":
                                                    foreach (XmlNode ESADoutDetails in bodyDetails.ChildNodes)
                                                    {
                                                        switch (ESADoutDetails.Name)
                                                        {
                                                            case "ESADout_CUGoodsShipment":
                                                                foreach (XmlNode shipmentDetails in ESADoutDetails.ChildNodes)
                                                                {
                                                                    switch (shipmentDetails.Name)
                                                                    {
                                                                        case "ESADout_CUGoods":
                                                                            ReadXmlDocumentRecordDTO dto = new() { DocumentName = readDocumentName };

                                                                            StringBuilder goods = new();

                                                                            foreach (XmlNode goodsItemDetails in shipmentDetails.ChildNodes)
                                                                            {
                                                                                switch (goodsItemDetails.Name)
                                                                                {
                                                                                    case "catESAD_cu:GoodsNumeric":
                                                                                        dto.Seq = int.TryParse(goodsItemDetails.InnerText, out int _seq) ? _seq : 0; break;
                                                                                    case "catESAD_cu:GoodsTNVEDCode":
                                                                                        dto.CommodityHSCode = goodsItemDetails.InnerText; break;
                                                                                    case "catESAD_cu:GoodsDescription":
                                                                                        goods.Append($"{goodsItemDetails.InnerText}" + ' '); break;   //dto.CommodityName = goodsItemDetails.InnerText; break;
                                                                                    case "catESAD_cu:GrossWeightQuantity":
                                                                                        dto.GrossWt = goodsItemDetails.InnerText; break;
                                                                                    case "catESAD_cu:NetWeightQuantity":
                                                                                        dto.NetWt = goodsItemDetails.InnerText; break;
                                                                                }

                                                                                dto.CommodityName = goods.ToString().Trim();
                                                                            }
                                                                            readDTOs.Add(dto);
                                                                            break;
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
            }

            #endregion
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); return null; }

        await Task.Delay(10);

        if (!readDTOs.Any()) return null;

        return readDTOs;
    }

    public void Dispose()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }
}
