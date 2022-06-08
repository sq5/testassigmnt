// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Diadoc.Api;
using Diadoc.Api.Proto.Documents;
using Diadoc.Api.Cryptography;
using Diadoc.Api.Proto.Events;
using System.IO;
using System.IO.Compression;
using Diadoc.Api.Proto;
using DATABASE.Context;
using System.Text;
using Diadoc.Api.DataXml.Utd820;

namespace CloudArchive.Services.EDI.EnsolDiadoc
{
    public class DiadocFileInfo
    {
        public string Name { get; set; }
        public byte[] BinaryData { get; set; }
    }

    public static class DiadocCommon
    {

        public static string[] FinalStatuses => new string[] { "OutgoingRecipientResponseStatusNotAcceptable", "OutgoingRecipientSignatureRequestRejected", "OutgoingRevocationAccepted", "OutgoingWithRecipientSignature", "IncomingRecipientResponseStatusNotAcceptable", "IncomingRecipientSignatureRequestRejected", "IncomingRevocationAccepted", "IncomingWithRecipientSignature" };


        public static List<DiadocFileInfo> DownloadFiles(string MessageID, string EntityID, string BoxID, Document di, DiadocJobSettings settings, bool DownloadMainFile)
        {

            List<DiadocFileInfo> files = new List<DiadocFileInfo>();
            string fn = FormName(di);
            try
            {
                PrintFormResult pfres = settings.ediSettings.Connection.GeneratePrintForm(settings.ediSettings.Token, BoxID, MessageID, EntityID);
                if (!pfres.HasContent)
                {
                    int timer = (pfres.RetryAfter + 1);
                    if (timer > 30)
                        timer = 30;
                    System.Threading.Thread.Sleep(timer * 1000);
                    pfres = settings.ediSettings.Connection.GeneratePrintForm(settings.ediSettings.Token, BoxID, MessageID, EntityID);
                }
                if (!pfres.HasContent)
                {
                    settings.generalSettings.LogService.AddError("Превышено время ожидания принтформы по документу MessageID: " + MessageID + " EntityID: " + EntityID, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
                }
                else
                    files.Add(new DiadocFileInfo() { BinaryData = pfres.Content.Bytes, Name = fn + "_ПечатнаяФорма.pdf" });
            }
            catch (Exception e)
            {
                settings.generalSettings.LogService.AddError("Произошла ошибка при запросе принтформы по документу " + MessageID + "_" + EntityID + " : " + e.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
            }
            IDocumentZipGenerationResult zres = settings.ediSettings.Connection.GenerateDocumentZip(settings.ediSettings.Token, BoxID, MessageID, EntityID, true);
            while (string.IsNullOrEmpty(zres.ZipFileNameOnShelf))
            {
                int timer = (zres.RetryAfter + 1);
                if (timer > 30)
                    timer = 30;
                System.Threading.Thread.Sleep(timer * 1000);
                zres = settings.ediSettings.Connection.GenerateDocumentZip(settings.ediSettings.Token, BoxID, MessageID, EntityID, true);
            }
            if (string.IsNullOrEmpty(zres.ZipFileNameOnShelf))
            {
                settings.generalSettings.LogService.AddError("Превышено время ожидания протокола по документу MessageID: " + MessageID + " EntityID: " + EntityID, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
            }
            else
                files.Add(new DiadocFileInfo() { BinaryData = settings.ediSettings.Connection.GetFileFromShelf(settings.ediSettings.Token, zres.ZipFileNameOnShelf), Name = "Протокол.zip" });
            if (DownloadMainFile)
            {
                byte[] data = settings.ediSettings.Connection.GetEntityContent(settings.ediSettings.Token, BoxID, MessageID, EntityID);
                files.Add(new DiadocFileInfo() { BinaryData = data, Name = di.FileName });
            }

            return files;
        }

        public static List<DiadocFileInfo> DownloadNamedFiles(string MessageID, string EntityID, string BoxID, bool isIncoming, Document di, DiadocJobSettings settings)
        {
            string fn = FormName(di);
            List<DiadocFileInfo> files = new List<DiadocFileInfo>();
            try
            {
                PrintFormResult pfres = settings.ediSettings.Connection.GeneratePrintForm(settings.ediSettings.Token, BoxID, MessageID, EntityID);
                if (!pfres.HasContent)
                {
                    int timer = (pfres.RetryAfter + 1);
                    if (timer > 30)
                        timer = 30;
                    System.Threading.Thread.Sleep(timer * 1000);
                    pfres = settings.ediSettings.Connection.GeneratePrintForm(settings.ediSettings.Token, BoxID, MessageID, EntityID);
                }
                if (!pfres.HasContent)
                {
                    settings.generalSettings.LogService.AddError("Превышено время ожидания принтформы по документу MessageID: " + MessageID + " EntityID: " + EntityID, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
                }
                else
                    files.Add(new DiadocFileInfo() { BinaryData = pfres.Content.Bytes, Name = fn + ".pdf" });
            }
            catch (Exception e)
            {
                settings.generalSettings.LogService.AddError("Произошла ошибка при запросе принтформы по документу " + MessageID + "_" + EntityID + " : " + e.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
            }
            if (CheckIfStatusIsFinal(di, isIncoming, out string status))
            {

                IDocumentZipGenerationResult zres = settings.ediSettings.Connection.GenerateDocumentZip(settings.ediSettings.Token, BoxID, MessageID, EntityID, true);
                if (string.IsNullOrEmpty(zres.ZipFileNameOnShelf))
                {
                    int timer = (zres.RetryAfter + 1);
                    if (timer > 30)
                        timer = 30;
                    System.Threading.Thread.Sleep(timer * 1000);
                    zres = settings.ediSettings.Connection.GenerateDocumentZip(settings.ediSettings.Token, BoxID, MessageID, EntityID, true);
                }
                if (string.IsNullOrEmpty(zres.ZipFileNameOnShelf))
                {
                    settings.generalSettings.LogService.AddError("Превышено время ожидания протокола по документу MessageID: " + MessageID + " EntityID: " + EntityID, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
                }
                else
                    files.Add(new DiadocFileInfo() { BinaryData = settings.ediSettings.Connection.GetFileFromShelf(settings.ediSettings.Token, zres.ZipFileNameOnShelf), Name = fn + ".zip" });
            }
            return files;
        }

        public static List<DiadocFileInfo> DownloadPackageFiles(string MessageID, string EntityID, string BoxID, bool isIncoming, Document di, DiadocJobSettings settings)
        {
            List<DiadocFileInfo> files = new List<DiadocFileInfo>();
            List<DiadocFileInfo> protocols = new List<DiadocFileInfo>();
            List<string> ProcessedIds = new List<string>();

            foreach (DiadocFileInfo df in DownloadNamedFiles(MessageID, EntityID, BoxID, isIncoming, di, settings))
            {
                if (df.Name.EndsWith(".zip"))
                {
                    protocols.Add(df);
                }
                else
                {
                    files.Add(df);
                }
            }
            ProcessedIds.Add(MessageID + EntityID);

            if (!string.IsNullOrEmpty(di.PacketId))
            {
                Diadoc.Api.Proto.Docflow.GetDocflowsByPacketIdRequest request = new Diadoc.Api.Proto.Docflow.GetDocflowsByPacketIdRequest();
                request.Count = 5;
                request.PacketId = di.PacketId;
                var result = settings.ediSettings.Connection.GetDocflowsByPacketId(settings.ediSettings.Token, BoxID, request);
                foreach (var packetDoc in result.Documents)
                {
                    var DocId = packetDoc.Document.DocumentId;

                    Document tmp = settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, DocId.MessageId, DocId.EntityId);

                    if (!ProcessedIds.Contains(DocId.MessageId + DocId.EntityId))
                    {
                        foreach (DiadocFileInfo df in DownloadNamedFiles(DocId.MessageId, DocId.EntityId, BoxID, isIncoming, tmp, settings))
                        {
                            if (df.Name.EndsWith(".zip"))
                            {
                                protocols.Add(df);
                            }
                            else
                            {
                                files.Add(df);
                            }
                        }
                        ProcessedIds.Add(DocId.MessageId + DocId.EntityId);
                    }
                }
            }
            files.Add(CreateZip(protocols));

            return files;
        }

        public static DiadocFileInfo CreateZip(List<DiadocFileInfo> fiels)
        {

            byte[] bytes = null;
            using (MemoryStream zipStream = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in fiels)
                    {
                        var entry = zip.CreateEntry(file.Name);
                        var fbytes = file.BinaryData;
                        using (Stream writer = entry.Open())
                        {
                            writer.Write(fbytes, 0, fbytes.Length);
                        }

                    }
                }
                zipStream.Position = 0;
                bytes = ReadFully(zipStream);
            }
            return new DiadocFileInfo() { BinaryData = bytes, Name = "Протокол.zip" };
        }

        public static DocFile AddorUpdateContractDocument(DiadocJobSettings settings, DiadocFileInfo document, Contract parent)
        {
            DocFile file = null;
            var ExistingDocs = settings.GeneralSettings.DbContext.Files.Where(x => x.ContractId == parent.Id && x.FileName == document.Name);
            if (ExistingDocs.Any())
            {
                file = ExistingDocs.FirstOrDefault();
                file.FileSize = document.BinaryData.Length;
                file.FileBin = document.BinaryData;
                file.Modified = DateTime.Now;
                settings.GeneralSettings.FileStorage.CreateOrUpdateAsync(file).GetAwaiter().GetResult();
            }
            else
            {
                file = new DocFile();
                file.FileName = document.Name;
                file.Created = DateTime.Now;
                file.Modified = DateTime.Now;
                file.NonFormDocId = 0;
                file.ContractId = parent.Id;
                file.MetaId = 0;
                file.FileSize = document.BinaryData.Length;
                file.FileBin = document.BinaryData;
                settings.GeneralSettings.FileStorage.CreateOrUpdateAsync(file).GetAwaiter().GetResult();
            }
            return file;
        }

        public static DocFile AddorUpdateMetadataDocument(DiadocJobSettings settings, DiadocFileInfo document, Metadata parent)
        {
            DocFile file = null;
            var ExistingDocs = settings.GeneralSettings.DbContext.Files.Where(x => x.MetaId == parent.Id && x.FileName == document.Name);
            if (ExistingDocs.Any())
            {
                file = ExistingDocs.FirstOrDefault();
                file.FileSize = document.BinaryData.Length;
                file.Modified = DateTime.Now;
                file.FileBin = document.BinaryData;
                settings.GeneralSettings.FileStorage.CreateOrUpdateAsync(file).GetAwaiter().GetResult();
            }
            else
            {
                file = new DocFile();
                file.FileName = document.Name;
                file.FileBin = document.BinaryData;
                file.Created = DateTime.Now;
                file.Modified = DateTime.Now;
                file.NonFormDocId = 0;
                file.ContractId = 0;
                file.MetaId = parent.Id;
                file.FileSize = document.BinaryData.Length;
                settings.GeneralSettings.FileStorage.CreateOrUpdateAsync(file).GetAwaiter().GetResult();
            }
            return file;
        }

        public static SignaturesAndEDIEvents AddSignaturesAndEDIEvents(DiadocJobSettings settings, string EventType, DateTime EventDate, string Comment = "", int ContractID = 0, long MetaId = 0, string System = "Диадок")
        {
            SignaturesAndEDIEvents eventNew = new SignaturesAndEDIEvents();

            eventNew.Signer = "System";
            eventNew.System = System;
            eventNew.EventDate = EventDate;

            eventNew.FileID = 0;
            eventNew.Approved = false;
            eventNew.SignatureBin = null;
            eventNew.Comment = Comment;

            eventNew.ContractID = ContractID;
            eventNew.MetaID = MetaId;
            eventNew.Event = EventType;

            settings.GeneralSettings.DbContext.SignaturesAndEDIEvents.Add(eventNew);

            return eventNew;
        }

        public static string GetContractorID(DiadocJobSettings settings, Contractor ctg)
        {
            DiadocSettings st = settings.ediSettings;
            return GetContractorID(st.Connection, st.ConnectionInfo, settings.generalSettings.DbContext, ctg, st.Token);
        }

        public static string GetContractorID(DiadocApi connection, EDISettings ConnectionInfo, SearchServiceDBContext db, Contractor ctg, string Token)
        {
            if (string.IsNullOrEmpty(ctg.DiadocID))
            {
                OrganizationList ol = connection.GetOrganizationsByInnKpp(ctg.INN, ctg.KPP);
                foreach (var org in ol.Organizations)
                {
                    if (org.Boxes.Count > 0)
                    {
                        Box b = org.Boxes[0];
                        ctg.DiadocID = b.BoxId;
                        db.SaveChanges();
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(ctg.DiadocID))
                throw new Exception(string.Format("Не удалось найти контрагента с ИНН {0}, КПП {1} в Диадок", ctg.INN, ctg.KPP));
            return ctg.DiadocID;
        }

        public static Contractor GetContractor(DiadocJobSettings settings, string ContractorBoxID)
        {
            Contractor ct = null;
            Box bb = settings.ediSettings.Connection.GetBox(ContractorBoxID);
            var ctorg = bb.Organization;

            var OrgID = settings.generalSettings.DbContext.Organizations.Where(x => x.ClientId == settings.ediSettings.ConnectionInfo.ClientID &&
            x.INN == settings.ediSettings.ConnectionInfo.OrganizationINN && x.KPP == settings.ediSettings.ConnectionInfo.OrganizationKPP).FirstOrDefault().Id;
            ct = settings.generalSettings.DbContext.Contractors.Where(x => x.OrganizationId == OrgID && x.INN == ctorg.Inn && (string.IsNullOrEmpty(ctorg.Kpp) || x.KPP == ctorg.Kpp)).FirstOrDefault();
            if (ct == null)
            {
                string Name = ctorg.FullName;
                Name = Name.Length > 100 ? Name.Substring(0, 100) : Name;
                ct = new Contractor();
                ct.Name = Name;
                ct.OrganizationId = OrgID;
                ct.INN = ctorg.Inn;
                ct.KPP = ctorg.Kpp;
                ct.DiadocID = bb.BoxId;
                settings.generalSettings.DbContext.Contractors.Add(ct);
                settings.generalSettings.DbContext.SaveChanges();
                return ct;
            }
            else
                return ct;
        }

        public static bool CheckIfStatusIsFinal(Document document, bool IsIncoming, out string status)
        {
            bool result = false;
            status = DiadocMapping.GetDocumentStatus(document, IsIncoming);
            if (FinalStatuses.Contains(status))
            {
                result = true;
            }
            status = DiadocMapping.GetDocumentStatusTranslated(status);
            return result;
        }

        public static bool Connect(DiadocJobSettings settings)
        {
            bool result = false;
            try
            {
                if (string.IsNullOrEmpty(settings.ediSettings.Token))
                {
                    WinApiCrypt crypt = new WinApiCrypt();
                    settings.ediSettings.Connection = new DiadocApi(settings.ediSettings.DiadocApiClientID, settings.generalSettings.Configuration["DiadocApiURL"], crypt);
                    settings.ediSettings.Token = settings.ediSettings.Connection.Authenticate(settings.ediSettings.ConnectionInfo.EDILogin, settings.ediSettings.ConnectionInfo.EDIPassword);
                }
                result = true;
            }
            catch (Exception e)
            {
                settings.ediSettings.Connection = null;
                settings.ediSettings.Token = "";
                settings.generalSettings.LogService.AddError(e.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
            }
            return result;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static string FormName(Document document)
        {
            string Filename = document.Title;
            if (Filename.EndsWith(".xml"))
            {
                string Number = document.DocumentNumber;
                if (string.IsNullOrEmpty(Number))
                    Number = "Б/Н";
                string Date = document.DocumentDate;
                if (string.IsNullOrEmpty(Date))
                    Date = "без даты";
                string Doctype = "Документ";
                if (DiadocMapping.DocTypes.ContainsKey(document.DocumentType.ToString()))
                    Doctype = DiadocMapping.DocTypes[document.DocumentType.ToString()];

                Filename = Doctype + "  №" + Number + " от " + Date;
            }
            else
            {
                if (Filename.Contains("."))
                    Filename = Filename.Substring(0, Filename.LastIndexOf("."));
            }
            return Filename;
        }

        public static string FindComment(DiadocJobSettings settings, Entity en, Message msg)
        {
            if (en.AttachmentType == AttachmentType.XmlSignatureRejection)
            {
                var title = settings.ediSettings.Connection.ParseSignatureRejectionXml(settings.ediSettings.Connection.GetEntityContent(settings.ediSettings.Token, settings.ediSettings.ConnectionInfo.EDIUserID, msg.MessageId, en.EntityId));
                return title.ErrorMessage;
            }
            else if (en.AttachmentType == AttachmentType.SignatureRequestRejection)
            {
                return Encoding.UTF8.GetString(settings.ediSettings.Connection.GetEntityContent(settings.ediSettings.Token, settings.ediSettings.ConnectionInfo.EDIUserID, msg.MessageId, en.EntityId));
            }
            else
            {
                foreach (Entity e in msg.Entities)
                    if (e.AttachmentType == AttachmentType.AttachmentComment && e.ParentEntityId == en.EntityId)
                    {
                        byte[] data = settings.ediSettings.Connection.GetEntityContent(settings.ediSettings.Token, settings.ediSettings.ConnectionInfo.EDIUserID, msg.MessageId, e.EntityId);
                        return Encoding.UTF8.GetString(data);
                    }
            }
            return "";
        }

        public static string GetTopParent(Message msg, string EnID)
        {
            string result = EnID.ToLower();
            foreach (Entity en in msg.Entities)
                if (en.EntityId.ToLower() == EnID.ToLower())
                    if (en.ParentEntityId != "")
                    {
                        result = GetTopParent(msg, en.ParentEntityId).ToLower();
                    }
            return result;
        }

        public static string GetTopParent(Message msg, Entity en)
        {
            string result = en.EntityId.ToLower();
            if (en.ParentEntityId != "")
            {
                result = GetTopParent(msg, en.ParentEntityId).ToLower();
            }
            return result;
        }
    }
}
