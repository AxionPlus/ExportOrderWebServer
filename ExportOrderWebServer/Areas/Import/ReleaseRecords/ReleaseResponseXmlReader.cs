using System.Xml;

namespace ExportOrderWebServer.Areas.Import.ReleaseRecords
{
    public class ReleaseResponseXmlReader
    {
        public string Num { get; set; }
        public DateTime? DocumentDated { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? Modification { get; set; }
        public string? Title { get; set; }
        public string? DocumentId { get; set; }
        public string? MessageId { get; set; }
        public string? ReleaseUID { get; set; }
        public string? AttributeName { get; set; }
        public string? AttributeValue { get; set; }
        public string? AttemptedAction { get; set; }
        public string? ErrorMessage { get; set; }
        public string? GeneratedDocumentID { get; set; }
        public bool SucceedRead { get; private set; } = false;

        public ReleaseResponseXmlReader(string text)
        {

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);

            if (doc.DocumentElement.Name == "documentImportStatus")
            {
                foreach (XmlNode childNode in doc.DocumentElement.ChildNodes)
                    switch (childNode.Name)
                    {
                        case "systemInfo":
                            try
                            {
                                var dateStr = childNode.FirstChild.InnerText.Split("T");
                                var date = dateStr[0].Split("-");
                                var time = dateStr[1].Split(":");
                                ActionDate = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ActionDate ERROR");
                                Console.WriteLine(ex.Message);
                            }
                            break;

                        case "importedDocumentInfo":
                            foreach (XmlNode documentInfoChildNode in childNode.ChildNodes)
                                switch (documentInfoChildNode.Name)
                                {
                                    case "documentNumber":
                                        Num = documentInfoChildNode.InnerText; break;
                                    case "documentDate":
                                        try
                                        {
                                            var dateStr = documentInfoChildNode.InnerText.Split("T");
                                            var date = dateStr[0].Split("-");
                                            var time = dateStr[1].Split(":");
                                            DocumentDated = new DateTime(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]), int.Parse(time[0]), int.Parse(time[1]), int.Parse(time[2]));
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("DocumentDate ERROR");
                                            Console.WriteLine(ex.Message);
                                        }
                                        break;
                                    case "documentTitle":
                                        Title = documentInfoChildNode.InnerText; break;
                                    case "documentModification":
                                        Modification = documentInfoChildNode.InnerText; break;
                                    case "documentId":
                                        DocumentId = documentInfoChildNode.InnerText; break;
                                    case "messageId":
                                        MessageId = documentInfoChildNode.InnerText; break;
                                    case "ReleaseUID":
                                        ReleaseUID = documentInfoChildNode.InnerText; break;
                                    case "documentAttributes":
                                        foreach (XmlNode attributesChildNode in documentInfoChildNode.ChildNodes)
                                            switch (attributesChildNode.Name)
                                            {
                                                case "attributeName":
                                                    AttributeName = attributesChildNode.InnerText; break;
                                                case "attributeValue":
                                                    AttributeValue = attributesChildNode.InnerText; break;
                                            }
                                        break;
                                }
                            break;

                        case "successInfo":
                            GeneratedDocumentID = childNode.FirstChild.InnerText; break;

                        case "failureInfo":
                            foreach (XmlNode failureInfoChildNode in childNode.ChildNodes)
                                switch (failureInfoChildNode.Name)
                                {
                                    case "attemptedAction":
                                        AttemptedAction = failureInfoChildNode.InnerText; break;
                                    case "errorMessage":
                                        ErrorMessage = failureInfoChildNode.InnerText; break;
                                }
                            break;
                    }

                SucceedRead = true;
            }
        }

    }
}
