using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Diadoc.Api.Proto.Documents;
using CloudArchive.Services.EDI;
using CloudArchive.Services.EDI.EnsolDiadoc;
using System.Threading;
using System.Globalization;
using Diadoc.Api.Proto.Events;
using Diadoc.Api.Proto;

namespace CloudArchive.ScheduledTasks
{
    public class DiadocExportMetadataWorker : IClientWorker, IClientMetadataWorker
    {
        private DiadocJobSettings settings { get; set; }
        public IEDIJobSettings Settings { get { return settings; } set { settings = value as DiadocJobSettings; } }
        public bool Completed { get; set; } = false;
        public List<Metadata> Documents { get; set; } = new List<Metadata>();
        public Metadata CurrentDocument { get; set; }

        public DiadocExportMetadataWorker(DiadocJobSettings sett)
        {
            Settings = sett;
        }

        public void ProcessBatch()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
            if (DiadocCommon.Connect(settings))
            {
                FindNextBatch();
                foreach (Metadata document in Documents)
                {
                    CurrentDocument = document;
                    ProcessDocument();
                }
                if (Documents.Count < settings.ClientSettings.TakeCount)
                {
                    Completed = true;
                }
                settings.ediSettings.Token = "";
            }
            else
            {
                Completed = true;
            }
        }

        public void FindNextBatch()
        {
            Documents = settings.generalSettings.DbContext.Metadatas
                .Where(x => x.ClientId == settings.EdiSettings.ConnectionInfo.ClientID &&
                x.EDIProvider == settings.EdiSettings.ConnectionInfo.EDIProvider &&
                x.EdiNeedExport == true &&
                x.Organization.INN == settings.EdiSettings.ConnectionInfo.OrganizationINN &&
                x.Organization.KPP == settings.EdiSettings.ConnectionInfo.OrganizationKPP &&
                x.Deleted == false)
                .Skip(settings.ClientSettings.Skip).Take(settings.ClientSettings.TakeCount).ToList();
        }

        public void ProcessDocument()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentDocument.EDIId))
                {
                    UploadContractNew();
                    DiadocCommon.AddSignaturesAndEDIEvents(settings, "Документ отправлен контрагенту в Диадок", DateTime.Now, "", 0, CurrentDocument.Id, "Endocs.Cloud");
                }
                else
                {
                    UploadNonformalizedSignature();
                    DiadocCommon.AddSignaturesAndEDIEvents(settings, "Подпись отправлена контрагенту в Диадок", DateTime.Now, "", 0, CurrentDocument.Id, "Endocs.Cloud");
                }
                CurrentDocument.EDIProcessed = true;
                settings.generalSettings.DbContext.SaveChanges();
                settings.generalSettings.LogService.AddInfo("Обновлен документ Ид: " + CurrentDocument.Id, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
            }
            catch (Exception e)
            {
                CurrentDocument.EDIState = "Ошибка отправки";
                CurrentDocument.Modified = DateTime.Now;
                CurrentDocument.EDIProcessed = true;
                CurrentDocument.EdiNeedExport = false;
                settings.generalSettings.LogService.AddError("Произошла ошибка во время выгрузки документа Ид: " + CurrentDocument.Id + " : " + e.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
                settings.generalSettings.DbContext.SaveChanges();

            }
        }

        private void UploadContractNew()
        {
            string BoxID = settings.EdiSettings.ConnectionInfo.EDIUserID;

            MessageToPost mess;
            Box bb;
            DiadocSettings st = settings.ediSettings;
            OrganizationList ol = st.Connection.GetMyOrganizations(st.Token);
            DocFile file = settings.generalSettings.DbContext.Files.Where(x => x.ContractId == CurrentDocument.Id).FirstOrDefault();

            bb = st.Connection.GetBox(BoxID);
            mess = new MessageToPost();
            mess.FromBoxId = BoxID;

            var CTCode = DiadocCommon.GetContractorID(settings, CurrentDocument.Contractor);
            mess.ToBoxId = CTCode;

            DocumentAttachment att = new DocumentAttachment();

            SignedContent content = new SignedContent();
            content.Content = settings.generalSettings.FileStorage.GetFileAsync(file).GetAwaiter().GetResult();
            content.Signature = GetSignatureSign(file);

            att.SignedContent = content;
            att.CustomDocumentId = CurrentDocument.Id.ToString();

            string summ = CurrentDocument.Amount.HasValue ? CurrentDocument.Amount.Value.ToString() : "0";

            att.Metadata.Add(new MetadataItem("FileName", file.FileName));
            att.Metadata.Add(new MetadataItem("DocumentNumber", CurrentDocument.DocNumber));
            if (CurrentDocument.DocDate.HasValue)
                att.Metadata.Add(new MetadataItem("DocumentDate", CurrentDocument.DocDate.Value.ToString("dd.MM.yyyy")));

            att.TypeNamedId = "Nonformalized";

            att.Comment = CurrentDocument.Comment;
            mess.AddDocumentAttachment(att);

            Message msg = st.Connection.PostMessage(st.Token, mess);
            SetNewFieldValues(msg);
        }

        private void UploadNonformalizedSignature()
        {
            string mixedID = CurrentDocument.EDIId;
            string MessageID = mixedID.Substring(0, mixedID.Length / 2);
            string EntityID = mixedID.Substring(mixedID.Length / 2);
            string BoxID = settings.EdiSettings.ConnectionInfo.EDIUserID;
            DiadocSettings st = settings.ediSettings;

            MessagePatchToPost mess;

            mess = new MessagePatchToPost();
            mess.BoxId = BoxID;
            mess.MessageId = MessageID;


            DocumentSignature sign = new DocumentSignature();

            sign.ParentEntityId = EntityID;
            DocFile file = settings.generalSettings.DbContext.Files.Where(x => x.ContractId == CurrentDocument.Id).FirstOrDefault();
            byte[] doccontent = settings.generalSettings.FileStorage.GetFileAsync(file).GetAwaiter().GetResult();

            sign.Signature = GetSignatureSign(file);
            mess.Signatures.Add(sign);

            var msg = st.Connection.PostMessagePatch(st.Token, mess);
            SetPatchFieldValues(msg);
        }

        private byte[] GetSignatureSign(DocFile file)
        {
            return settings.generalSettings.DbContext.SignaturesAndEDIEvents.Where(x => x.FileID == file.Id).FirstOrDefault().SignatureBin;
        }


        private void SetNewFieldValues(Message msg)
        {
            for (int i = 0; i < msg.EntitiesList.Count; i++)
            {
                Entity en = msg.EntitiesList.Item(i) as Entity;
                if (en.EntityType == EntityType.Attachment && en.DocumentInfo != null)
                {
                    string MessageId = msg.MessageId;
                    string EntityId = en.EntityId;
                    string DocID = MessageId + EntityId;
                    string BoxID = settings.EdiSettings.ConnectionInfo.EDIUserID;
                    string status = "";
                    Document di = settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, MessageId, EntityId);
                    var IsIncoming = msg.ToBoxId.ToLower() == BoxID.ToLower();

                    CurrentDocument.EDIProcessed = DiadocCommon.CheckIfStatusIsFinal(di, true, out status);
                    CurrentDocument.EDIState = status;
                    CurrentDocument.EDIId = DocID;
                    CurrentDocument.EDIIsIncoming = IsIncoming;

                    CurrentDocument.EDIProvider = "Диадок";
                    CurrentDocument.Modified = DateTime.Now;
                }
            }
        }

        private void SetPatchFieldValues(MessagePatch msg)
        {
            for (int i = 0; i < msg.EntitiesList.Count; i++)
            {
                Entity en = msg.EntitiesList.Item(i) as Entity;
                if (en.EntityType == EntityType.Attachment && en.DocumentInfo != null)
                {
                    string MessageId = msg.MessageId;
                    string EntityId = en.EntityId;
                    string BoxID = settings.EdiSettings.ConnectionInfo.EDIUserID;
                    string status = "";
                    Document di = settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, MessageId, EntityId);

                    CurrentDocument.EDIProcessed = DiadocCommon.CheckIfStatusIsFinal(di, false, out status);
                    CurrentDocument.EDIState = status;
                    CurrentDocument.Modified = DateTime.Now;
                }
            }
        }
    }
}
