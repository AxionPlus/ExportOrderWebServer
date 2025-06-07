using ExportOrderEntites.ReleaseRecord;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using static ExportOrderWebServer.Areas.Import.ReleaseRecords.Services.ReleaseService;

namespace ExportOrderWebServer.Service
{
    public class Class
    {
        //public async Task<AppObjectResponse> SendEmailReleaseImportXmlAsync()
        //{
        //    var completedList = new List<ReleaseImportXmlDTO>();

        //    string tempFileDir = Path.Combine(_webHostEnvironment.ContentRootPath, "Temp_Files");

        //    var ConfigurationEmail = _configuration.GetSection("SendReleaseEmailDetailsSMTP").Get<SendReleaseEmailDetails>();
        //    if (ConfigurationEmail is null)
        //        return appObjResponse;

        //    using (var _db = _dbContext.CreateDbContextAsync())
        //    {
        //        var db = await _db;

        //        var NeedSendTerminalReleaseRecords = await db.Set<ReleaseImportContainerRecord>().AsNoTracking().Where(s => s.ReleaseStatus == ReleaseStatus.NeedSendTerminal).ToArrayAsync();

        //        if (NeedSendTerminalReleaseRecords.Count() == 0) return new();


        //        using (var smtpClient = new SmtpClient())
        //        {
        //            smtpClient.Timeout = 214748364;
        //            smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
        //            await smtpClient.ConnectAsync(ConfigurationEmail.Host, ConfigurationEmail.Port, false);
        //            smtpClient.Authenticate(ConfigurationEmail.EmailAddress, ConfigurationEmail.Password);



        //            foreach (var releaseRecord in NeedSendTerminalReleaseRecords)
        //            {
        //                // Create xml File
        //                string fileName = $"{releaseRecord.DocNumber}_{releaseRecord.BillOfLadingNo}_{releaseRecord.ContainerNo}.xml";
        //                string filePath = Path.Combine(tempFileDir, fileName.Replace("/", "-"));           // путь к файлу

        //                CreateReleaseXMLfile(filePath, releaseRecord);

        //                #region Sending Email
        //                var message = new MimeMessage();
        //                message.From.Add(new MailboxAddress(ConfigurationEmail.Name, ConfigurationEmail.EmailAddress));

        //                message.To.Add(new MailboxAddress("NLE", "release-marmed@ct.nle.ru"));
        //                message.Cc.Add(new MailboxAddress("Import Dept", "import@portservis.com"));
        //                message.Bcc.Add(new MailboxAddress("get_api_email@mail.ru", "get_api_email@mail.ru"));
        //                //message.ReplyToList.Add(new MailAddress("Fin@neo-line.ru", "FIN NEO LINE AGENCY"));
        //                message.Subject = $"Релиз от {releaseRecord.CreateTime.ToShortDateString()} к/с {releaseRecord.BillOfLadingNo} кнтр {releaseRecord.ContainerNo}";

        //                var bodyBuilder = new BodyBuilder();
        //                bodyBuilder.HtmlBody = @$"<html xmlns= http://www.w3.org/1999/xhtml >" +
        //                                "<body>" +
        //                                "В приложении релиз.<br/>" +
        //                                "</body>" +
        //                                "</html>"; ;


        //                if (File.Exists(filePath))
        //                    bodyBuilder.Attachments.Add(filePath);

        //                message.Body = bodyBuilder.ToMessageBody();

        //                try
        //                {
        //                    await smtpClient.SendAsync(message);
        //                    message.Dispose();
        //                    Console.WriteLine($"Sent {releaseRecord.ContainerNo}");
        //                }
        //                catch (Exception ex)
        //                {
        //                    message.Dispose();

        //                    if (System.IO.File.Exists(filePath))
        //                        System.IO.File.Delete(filePath);

        //                    string msg = ex.Message;
        //                    Console.WriteLine(msg);

        //                    appObjResponse.Object = completedList;
        //                    return appObjResponse;
        //                }
        //                #endregion


        //                releaseRecord.ReleaseStatus = ReleaseStatus.SentTerminal;

        //                db.Entry(releaseRecord).State = EntityState.Modified;


        //                // delete Temp file
        //                if (System.IO.File.Exists(filePath))
        //                    System.IO.File.Delete(filePath);

        //                await Task.Delay(5000);
        //            }

        //            var bug = db.ChangeTracker.DebugView.LongView;
        //            await db.SaveChangesAsync();
        //            db.ChangeTracker.Clear();
        //        }
        //    }

        //    appObjResponse.Object = completedList;
        //    return appObjResponse;
        //}

        //public void CreateReleaseXMLfile(string path, ReleaseImportContainerRecord releaseItem)
        //{
        //    if (string.IsNullOrEmpty(path)) return;
        //    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //    try
        //    {
        //        using (XmlTextWriter xml = new XmlTextWriter(path, Encoding.GetEncoding("windows-1251")))  // Encoding.UTF8
        //        {

        //            xml.Formatting = Formatting.Indented;
        //            xml.WriteStartDocument();

        //            xml.WriteStartElement("ReleaseOrder");
        //            xml.WriteAttributeString("xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "ReleaseOrder.xsd");

        //            xml.WriteStartElement("DocHead");
        //            xml.WriteElementString("DocName", "ReleaseOrder");
        //            xml.WriteElementString("DocNumber", releaseItem.DocNumber);
        //            xml.WriteElementString("DocDate", DateReverse(DateTime.Now, true));
        //            xml.WriteElementString("Modification", releaseItem.ReleaseMode.ToString());
        //            xml.WriteEndElement();

        //            xml.WriteStartElement("Stevedore");
        //            xml.WriteElementString("Name", releaseItem.TerminalName);
        //            xml.WriteEndElement();

        //            ///< LINE >
        //            ///< Name > Код линии </ Name >
        //            ///</ LINE >
        //            xml.WriteStartElement("LINE");
        //            xml.WriteElementString("Name", "MARMED");
        //            xml.WriteEndElement();

        //            //xml.WriteStartElement("Agent");
        //            //xml.WriteElementString("Name", releaseItem.AgreementContractor); //xml.WriteElementString("Name", releaseItem.AgreementContractor);<Name></Name>
        //            //xml.WriteElementString("ContractID", releaseItem.AgreementNo); //xml.WriteElementString("ContractID", releaseItem.AgreementNo);
        //            //xml.WriteEndElement();

        //            xml.WriteElementString("ReleaseUID", releaseItem.ReleaseUID);
        //            xml.WriteElementString("ExpireDate", DateReverse(releaseItem.ReleaseTo, false));

        //            xml.WriteStartElement("Forwarder");
        //            xml.WriteElementString("ForwarderINN", "1234567890");
        //            xml.WriteEndElement();

        //            xml.WriteStartElement("Container");
        //            xml.WriteElementString("Prefix", releaseItem.ContainerNo is null ? string.Empty : releaseItem.ContainerNo.Substring(0, 4));
        //            xml.WriteElementString("Number", releaseItem.ContainerNo is null ? string.Empty : releaseItem.ContainerNo.Substring(4));
        //            xml.WriteElementString("Type", releaseItem.ContainerType);
        //            xml.WriteEndElement();

        //            xml.WriteEndElement();

        //            xml.Close();
        //        }

        //        byte[] fileBytes = File.ReadAllBytes(path);

        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = ex.Message;
        //        Console.WriteLine(msg);
        //    }
        //}

    }
}
