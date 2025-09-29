using ExportOrderEntites.ReleaseRecord;
using ExportOrderWebServer.Areas.Import.ReleaseRecords.Dto;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml;
using ExportOrderEntites.EmailLogRecords;

namespace ExportOrderWebServer.Areas.Import.ReleaseRecords.Services
{
    public interface IReleaseService
    {
        public Task SendEmailReleaseImportConfirmationToCustomerAsync();
        public Task CheckEmailBoxReleaseImportAsync(ApplicationDbContext db);
        public Task CheckReleaseImportResponseAsync(ApplicationDbContext db);
        public Task SendEmailReleaseImportXmlAsync(ApplicationDbContext dbContext);
    }

    public class ReleaseService : IReleaseService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private AppObjectResponse appObjResponse;

        public ReleaseService(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration
           /* IDbContextFactory<ApplicationDbContext> dbContext*/)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            //_dbContext = dbContext;
            appObjResponse = new();
        }

        internal class SendReleaseEmailDetails
        {
            public string Name { get; set; }
            public string EmailAddress { get; set; }
            public string Password { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string MessegaCC { get; set; }
            public string MessegaTo { get; set; }

        }
        private (string, string) CatchAttachment(MimeMessage message, string[] FileExtensions)
        {
            string fileName = string.Empty;

            foreach (var attachment in message.Attachments)
                using (var memStream = new MemoryStream())
                {
                    {
                        if (attachment is MessagePart)
                        {
                            fileName = attachment.ContentDisposition.FileName;
                            var rfc822 = (MessagePart)attachment;
                            foreach (var fileExtension in FileExtensions)
                                if (!fileName.ToLower().Contains(fileExtension)) continue;
                            rfc822.Message.WriteTo(memStream);
                        }
                        else
                        {
                            var part = (MimePart)attachment;
                            fileName = part.FileName;
                            foreach (var fileExtension in FileExtensions)
                                if (!fileName.ToLower().Contains(fileExtension)) continue;
                            try
                            {
                                part.Content.DecodeTo(memStream);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("part.Content.DecodeTo(memStream);");
                            }
                        }

                        using (var MemStreamReader = new StreamReader(memStream))
                        {
                            memStream.Position = 0;
                            string text = MemStreamReader.ReadToEnd();

                            if (fileName.ToUpper().Contains("XML"))
                                return (fileName.ToUpper(), text);
                            if (fileName.ToUpper().Contains("EDI"))
                                return (fileName.ToUpper(), text);
                        }
                    }
                }
            return (string.Empty, string.Empty);
        }//string RetrieveEdiMessage(MimeMessage message, Stream SaveTo)

        public async Task CheckEmailBoxReleaseImportAsync(ApplicationDbContext db)
        {
            var appObjectResponse = new AppObjectResponse();
            var AppSettings = _configuration.GetSection("ReadReleaseEmailDetailsIMAP").Get<SendReleaseEmailDetails>();
            if (AppSettings is null)
                return;
            try
            {
                using var client = new ImapClient();
                client.Timeout = 10000000;
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(AppSettings.Host, AppSettings.Port, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(AppSettings.EmailAddress, AppSettings.Password);


                //параметры поиска      SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-(AppSettings.FetchDays)))
                SearchQuery searchQuery = SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-10))
                    .And(SearchQuery.FromContains("dnle@ct.nle.ru"))
                    .And(SearchQuery.NotSeen);

                // Получаем и открываем папку releaseNle
                var releaseNleFolder = await client.Inbox.GetSubfolderAsync("releaseNle");
                await releaseNleFolder.OpenAsync(FolderAccess.ReadWrite);

                // Выполняем поиск в нужной папке
                var uids = await releaseNleFolder.SearchAsync(searchQuery);
                if (!uids.Any())
                    return;

                Console.WriteLine("uids " + uids.Count);

                var items = MessageSummaryItems.BodyStructure | MessageSummaryItems.UniqueId;
                var matched = new UniqueIdSet(); //targeted list - messages with attachment(s)
                foreach (var msg in await releaseNleFolder.FetchAsync(uids, items))
                {
                    if (msg.BodyParts.Any(x => x.IsAttachment)) matched.Add(msg.UniqueId);
                }
                //time to retrieve attachemnts
                if (matched is null) return;
                Console.WriteLine("matched " + matched.Count);

                MimeMessage message;
                IMailFolder DestinationFolder = null;


                foreach (var item in matched)
                {
                    message = await releaseNleFolder.GetMessageAsync(item);

                    var fileContent = CatchAttachment(message, new[] { "xml" });

                    if (string.IsNullOrWhiteSpace(fileContent.Item1))
                        continue;
                    var emailLogRecord = new EmailLogRecord()
                    {
                        EmailSubject = message.Subject.ToString(),
                        From = message.From.ToString(),
                        DateReciept = message.Date.DateTime,
                        FileContent = fileContent.Item2,
                        FileName = fileContent.Item1,
                        Type = EmailLogRecordType.ResponseTerminal,
                    };

                    db.Entry(emailLogRecord).State = EntityState.Added;
                    await db.SaveChangesAsync();
                    db.ChangeTracker.Clear();

                    await releaseNleFolder.AddFlagsAsync(item, MessageFlags.Seen, true);


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                appObjectResponse.ErrorAdd(ex.Message);
            }

        }

        public async Task CheckReleaseImportResponseAsync(ApplicationDbContext db)
        {

            var Emails = await db.Set<EmailLogRecord>().AsNoTracking()
                .Where(s => s.Type == EmailLogRecordType.ResponseTerminal)
                .Where(s => !s.IsHandle)
                .ToArrayAsync();

            if (!Emails.Any()) return;


            foreach (var item in Emails)
            {
                if (item.FileName.Contains("XML"))
                {
                    var response = new ReleaseResponseXmlReader(item.FileContent);

                    if (response is not null && response.SucceedRead == true)
                    {
                        var sentItems = await db.Set<ReleaseImportContainerRecordEntity>()
                                                                                    // .Include(s => s.SendingCustomerRecords.Where(r => r.Mode == ImportReleaseMailSendingMode.WaitResponse))
                                                                                    .AsNoTracking()
                                                                                    .OrderByDescending(s => s.CreatedAt)
                                                                                    .Where(s => s.ReleaseStatus == ReleaseStatus.SentTerminal)
                                                                                    .Where(s => s.DocNumber == response.DocumentId)
                                                                                    .ToArrayAsync();
                        var sentItem = sentItems.FirstOrDefault();
                        if (sentItem is not null)
                        {
                            if (sentItem.ReleaseMode == ReleaseMode.Create)
                            {
                                if (!string.IsNullOrWhiteSpace(response.GeneratedDocumentID))
                                {
                                    sentItem.ReleaseStatus = ReleaseStatus.ConfirmedTerminal;

                                    //foreach (var sentItemCustomerRecord in sentItem.SendingCustomerRecords)
                                    //{
                                    //    sentItemCustomerRecord.Mode = ImportReleaseMailSendingMode.NeedSend;
                                    //    db.Entry(sentItemCustomerRecord).State = EntityState.Modified;
                                    //}
                                }
                            }
                            else if (sentItem.ReleaseMode == ReleaseMode.Reject)
                            {
                                sentItem.ReleaseStatus = ReleaseStatus.XCancelled;
                                db.Entry(new ReleaseRemark()
                                {
                                    BillOfLadingNum = sentItem.BillOfLadingNum,
                                    ContainerNum = sentItem.ContainerNum,
                                    DocNumber = sentItem.DocNumber,
                                    Remark = "подтверждение отмены релиза",
                                    CreatedAt = DateTimeOffset.Now,
                                    Timestamp = DateTime.Now.Ticks,

                                }).State = EntityState.Added;
                            }
                            //sentItem.ReleaseStatus = ReleaseStatus.ConfirmedTerminal;
                            //foreach (var sentItemCustomerRecord in sentItem.SendingCustomerRecords)
                            //{
                            //    sentItemCustomerRecord.Mode = ImportReleaseMailSendingMode.NeedSend;
                            //    db.Entry(sentItemCustomerRecord).State = EntityState.Modified;
                            //}



                            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                            {
                                ///Номинация экспедитора для контейнера 'SEGU1968156' истекла
                                ///Номинация экспедитора для контейнера .* истекла 
                                var reg = Regex.Match(response.ErrorMessage, "Номинация экспедитора для контейнера .* истекла ");
                                if (reg.Success)
                                {
                                    db.Entry(new ReleaseRemark()
                                    {
                                        BillOfLadingNum = sentItem.BillOfLadingNum,
                                        ContainerNum = sentItem.ContainerNum,
                                        DocNumber = sentItem.DocNumber,
                                        Remark = response.ErrorMessage,
                                        CreatedAt = DateTimeOffset.Now,
                                        Timestamp = DateTime.Now.Ticks,

                                    }).State = EntityState.Added;
                                }

                                /// Вы пытаетесь добавить контейнер 'EIAU2602895', который уже заявлен в документе 'FDR3499'
                                ///Вы пытаетесь добавить контейнер .*, который уже заявлен в документе 
                                reg = Regex.Match(response.ErrorMessage, "Вы пытаетесь добавить контейнер .*, который уже заявлен в документе");
                                if (reg.Success)
                                {
                                    sentItem.TerminalErrorResponse = response.ErrorMessage;

                                    db.Entry(new ReleaseRemark()
                                    {
                                        BillOfLadingNum = sentItem.BillOfLadingNum,
                                        ContainerNum = sentItem.ContainerNum,
                                        DocNumber = sentItem.DocNumber,
                                        Remark = response.ErrorMessage,
                                        CreatedAt = DateTimeOffset.Now,
                                        Timestamp = DateTime.Now.Ticks,

                                    }).State = EntityState.Added;
                                }
                                // sentItem.Status = EntityStatus.XCancelled;

                                //foreach (var sentItemCustomerRecord in sentItem.SendingCustomerRecords)
                                //{
                                //    sentItemCustomerRecord.Mode = ImportReleaseMailSendingMode.XCancelled;
                                //    db.Entry(sentItemCustomerRecord).State = EntityState.Modified;
                                //}

                            }

                            db.Entry(sentItem).State = EntityState.Modified;
                            item.IsHandle = true;
                            db.Entry(item).State = EntityState.Modified;

                            await db.SaveChangesAsync();
                            db.ChangeTracker.Clear();
                        }
                        else
                        {
                            appObjResponse.ErrorAdd($"Release {response.ReleaseUID} not found.");
                            if (item.CreateTime.AddDays(1) <= DateTime.Now)
                            {
                                item.IsHandle = true;
                                db.Entry(item).State = EntityState.Modified;
                                await db.SaveChangesAsync();
                            }
                        }

                    }
                }
            }


        }

        public void CreateReleaseXMLfile(string path, ReleaseImportContainerRecordEntity releaseItem)
        {
            if (string.IsNullOrEmpty(path)) return;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
                using (XmlTextWriter xml = new XmlTextWriter(path, Encoding.GetEncoding("windows-1251")))  // Encoding.UTF8
                {

                    xml.Formatting = Formatting.Indented;
                    xml.WriteStartDocument();

                    xml.WriteStartElement("ReleaseOrder");
                    xml.WriteAttributeString("xsi", "noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "ReleaseOrder.xsd");

                    xml.WriteStartElement("DocHead");
                    xml.WriteElementString("DocName", "ReleaseOrder");
                    xml.WriteElementString("DocNumber", releaseItem.DocNumber);
                    xml.WriteElementString("DocDate", DateReverse(DateTime.Now, true));
                    xml.WriteElementString("Modification", releaseItem.ReleaseMode.ToString());
                    xml.WriteEndElement();

                    xml.WriteStartElement("Stevedore");
                    xml.WriteElementString("Name", releaseItem.TerminalName);
                    xml.WriteEndElement();

                    ///< LINE >
                    ///< Name > Код линии </ Name >
                    ///</ LINE >
                    xml.WriteStartElement("LINE");
                    xml.WriteElementString("Name", releaseItem.LineName);
                    xml.WriteEndElement();

                    //xml.WriteStartElement("Agent");
                    //xml.WriteElementString("Name", releaseItem.AgreementContractor); //xml.WriteElementString("Name", releaseItem.AgreementContractor);<Name></Name>
                    //xml.WriteElementString("ContractID", releaseItem.AgreementNo); //xml.WriteElementString("ContractID", releaseItem.AgreementNo);
                    //xml.WriteEndElement();

                    xml.WriteElementString("ReleaseUID", releaseItem.ReleaseUID);
                    xml.WriteElementString("ExpireDate", DateReverse(releaseItem.ReleaseTo, false));

                    xml.WriteStartElement("Forwarder");
                    xml.WriteElementString("ForwarderINN", "1234567890");
                    xml.WriteEndElement();

                    xml.WriteStartElement("Container");
                    xml.WriteElementString("Prefix", releaseItem.ContainerNum is null ? string.Empty : releaseItem.ContainerNum.Substring(0, 4));
                    xml.WriteElementString("Number", releaseItem.ContainerNum is null ? string.Empty : releaseItem.ContainerNum.Substring(4));
                    xml.WriteElementString("Type", releaseItem.ContainerType);
                    xml.WriteEndElement();

                    xml.WriteEndElement();

                    xml.Close();
                }

                byte[] fileBytes = File.ReadAllBytes(path);

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                Console.WriteLine(msg);
            }
        }

        public Task SendEmailReleaseImportConfirmationToCustomerAsync()
        {
            throw new NotImplementedException();
        }

        public async Task SendEmailReleaseImportXmlAsync(ApplicationDbContext db)
        {
            var completedList = new List<ReleaseRecordDto>();

            string tempFileDir = Path.Combine(_webHostEnvironment.ContentRootPath, "Temp_Files");
            if (!Directory.Exists(tempFileDir))
                Directory.CreateDirectory(tempFileDir);

            var ConfigurationEmail = _configuration.GetSection("SendReleaseEmailDetailsSMTP").Get<SendReleaseEmailDetails>();
            if (ConfigurationEmail is null)
                return;

            //using (var _db = _dbContext.CreateDbContextAsync())
            //{
            //var db = await _db;

            var NeedSendTerminalReleaseRecords = await db.Set<ReleaseImportContainerRecordEntity>()
                .AsNoTracking()
                .Where(s => s.ReleaseStatus == ReleaseStatus.NeedSendTerminal)
                .ToArrayAsync();

            if (!NeedSendTerminalReleaseRecords.Any()) return;

            int loops = (int)Math.Ceiling((double)NeedSendTerminalReleaseRecords.Length / 25);
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Timeout = 214748364;
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await smtpClient.ConnectAsync(ConfigurationEmail.Host, ConfigurationEmail.Port, false);
                smtpClient.Authenticate(ConfigurationEmail.EmailAddress, ConfigurationEmail.Password);

                for (int loop = 1; loop <= loops; loop++)
                {
                    var loopList = NeedSendTerminalReleaseRecords.Skip((loop - 1) * 25).Take(25 * loop);
                    var filePaths = new List<string>();

                    foreach (var releaseRecord in loopList)
                    {
                        string fileName =
                            $"{releaseRecord.DocNumber}_{releaseRecord.BillOfLadingNum}_{releaseRecord.ContainerNum}.xml";
                        string filePath = Path.Combine(tempFileDir, fileName.Replace("/", "-")); // путь к файлу
                        filePaths.Add(filePath);
                        CreateReleaseXMLfile(filePath, releaseRecord);
                    }


                    #region Sending Email

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(ConfigurationEmail.Name, ConfigurationEmail.EmailAddress));

                    message.To.Add(new MailboxAddress("NLE",
                        ConfigurationEmail.MessegaTo)); //release-soling-agency@ct.nle.ru
                    message.Cc.Add(new MailboxAddress("Import Dept", ConfigurationEmail.MessegaCC));
                    message.Bcc.Add(new MailboxAddress("get_api_email@mail.ru", "get_api_email@mail.ru"));
                    //message.ReplyToList.Add(new MailAddress("Fin@neo-line.ru", "FIN NEO LINE AGENCY"));
                    message.Subject = $"Релиз от к/с {DateTime.Now.Ticks}";

                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = @$"<html xmlns= http://www.w3.org/1999/xhtml >" +
                                           "<body>" +
                                           "В приложении релиз.<br/>" +
                                           "</body>" +
                                           "</html>";
                    ;

                    foreach (var filePath in filePaths)
                        if (File.Exists(filePath))
                            bodyBuilder.Attachments.Add(filePath);

                    message.Body = bodyBuilder.ToMessageBody();

                    try
                    {
                        await smtpClient.SendAsync(message);
                        message.Dispose();

                        foreach (var filePath in filePaths)
                            if (System.IO.File.Exists(filePath))
                                System.IO.File.Delete(filePath);

                        foreach (var releaseRecord in loopList)
                        {
                            releaseRecord.ReleaseStatus = ReleaseStatus.SentTerminal;
                            releaseRecord.UpdatedAt = DateTimeOffset.Now;

                            db.Entry(releaseRecord).State = EntityState.Modified;
                        }

                        var bug = db.ChangeTracker.DebugView.LongView;
                        await db.SaveChangesAsync();
                        db.ChangeTracker.Clear();

                    }
                    catch (Exception ex)
                    {
                        message.Dispose();
                        foreach (var filePath in filePaths)
                            if (System.IO.File.Exists(filePath))
                                System.IO.File.Delete(filePath);

                        string msg = ex.Message;
                        Console.WriteLine(msg);

                        appObjResponse.Object = completedList;
                        return;
                    }

                    #endregion



                }


            }
            //}
        }

        private string DateReverse(DateTime _date, bool _isTimeIncluded)
        {
            string date = _date.ToString("yyyy") + "-" + _date.ToString("MM") + "-" + _date.ToString("dd") + "T";
            string time = "23:59:59";

            if (_isTimeIncluded)
                time = _date.ToString("HH") + ":" + _date.ToString("mm") + ":" + _date.ToString("ss");

            return date + time;
        }

    }
}
