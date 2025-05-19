using System.Xml;

namespace ExportOrderWebServer.Service;

public interface IXmlFileReadService : IDisposable
{
    public Task<List<ReadXmlDocumentDTO>?> ReadXmlFileDocuments(string dirPath);
}

public class XmlFileReadService : IXmlFileReadService
{
    private static string? DirPath { get; set; }

    public XmlFileReadService() { }

    public async Task<List<ReadXmlDocumentDTO>?> ReadXmlFileDocuments(string dirPath)
    {
        DirPath = dirPath;

        List<ReadXmlDocumentDTO> readListXmlDTO = new();

        foreach (var file in Directory.GetFiles(DirPath))
        {
            if (!File.Exists(file))
            {
                continue;
                throw new FileNotFoundException(file);
            }            

            XmlDocument doc = new();

            //doc.LoadXml(file);
            doc.Load(file);

            if (doc.DocumentElement == null || !doc.DocumentElement.Name.Contains("GoodsDeclaration"))
                return null;

            foreach(XmlNode childNode in doc.DocumentElement.ChildNodes)
            {
                ReadXmlDocumentDTO DTO = new();

                switch (childNode.Name)
                {
                    case "casdo:DeclarationKindCode":
                        DTO.DocumentType = childNode.InnerText; break;

                    case "cacdo:GDGoodsShipmentDetails":
                        foreach (XmlNode shipmentDetails in childNode.ChildNodes)
                        {
                            string shipper = string.Empty;
                            string consignee = string.Empty;                            

                            switch (shipmentDetails.Name)
                            {
                                case "cacdo:ConsignorDetails":
                                    DTO.ShipperName = shipmentDetails.InnerText; break;
                                
                                case "cacdo:ConsigneeDetails":
                                    foreach (XmlNode consigneeDetails in shipmentDetails.ChildNodes)
                                    {
                                        switch (consigneeDetails.Name)
                                        {
                                            case "csdo:SubjectBriefName":
                                                consignee = consigneeDetails.InnerText; break;

                                            case "ccdo:SubjectAddressDetails":
                                                foreach (XmlNode consigneeAddress in consigneeDetails.ChildNodes)
                                                {
                                                    switch (consigneeAddress.Name)
                                                    {
                                                        case "csdo:PostCode":
                                                            consignee = string.Concat(consignee, ", ", consigneeAddress.InnerText); break;
                                                        case "csdo:BuildingNumberId":
                                                            consignee = string.Concat(consignee, ", ", consigneeAddress.InnerText); break;
                                                        case "csdo:StreetName":
                                                            consignee = string.Concat(consignee, ", ", consigneeAddress.InnerText); break;
                                                        case "csdo:CityName":
                                                            consignee = string.Concat(consignee, ", ", consigneeAddress.InnerText); break;
                                                        case "csdo:RegionName":
                                                            consignee = string.Concat(consignee, ", ", consigneeAddress.InnerText); break;
                                                        case "UnifiedCountryCode":
                                                            DTO.ConsigneeCountry = consigneeAddress.InnerText; break;
                                                    }
                                                    DTO.ConsigneeName = consignee;
                                                }
                                                break;
                                        }
                                    }
                                    break;

                                case "cacdo:GDGoodsItemDetails":
                                    
                                    //int indexRecord = 0;

                                    foreach (XmlNode goodsItemDetails in shipmentDetails.ChildNodes)
                                    {
                                        ReadXmlDocumentRecordsDTO recordDTO = new();

                                        //recordDTO.Seq = ++indexRecord;
                                        ++recordDTO.Seq;

                                        switch (goodsItemDetails.Name)
                                        {
                                            case "csdo:CommodityCode":
                                                recordDTO.CommodityHSCode = goodsItemDetails.InnerText; break;
                                            case "csdo:GoodsDescriptionText":
                                                recordDTO.CommodityName = goodsItemDetails.InnerText; break;
                                            case "csdo:UnifiedGrossMassMeasure":
                                                recordDTO.GrossWt = goodsItemDetails.InnerText; break;
                                            case "csdo:UnifiedNetMassMeasure":
                                                recordDTO.NetWt = goodsItemDetails.InnerText; break;
                                            //case "csdo:GoodsItemGroupDetails":
                                            //    foreach (XmlNode goodsItemGroupDetails in goodsItemDetails.ChildNodes)
                                            //    {

                                            //    }
                                            //    break;
                                        }

                                       DTO.Records.Add(recordDTO);
                                    }
                                    break;

                                case "cacdo:PrecedingDocDetails":
                                    DTO.Name = shipmentDetails.InnerText; break;                                    
                            }                            
                        }
                        break;
                }

                readListXmlDTO.Add(DTO);
            }            
        }

        await Task.Delay(1000);

        if (readListXmlDTO is not null && readListXmlDTO.Any())
            return readListXmlDTO;
        else
            return null;
    }

    public void Dispose()
    {
        if (Directory.Exists(DirPath))
            Directory.Delete(DirPath, true);
    }
}
