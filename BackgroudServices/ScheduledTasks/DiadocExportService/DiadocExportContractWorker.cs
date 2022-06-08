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
    public class DiadocExportContractWorker : IClientWorker, IClientContractWorker
    {
        private DiadocJobSettings settings { get; set; }
        public IEDIJobSettings Settings { get { return settings; } set { settings = value as DiadocJobSettings; } }
        public bool Completed { get; set; } = false;
        public List<Contract> Documents { get; set; } = new List<Contract>();
        public Contract CurrentDocument { get; set; }

        private SignaturesAndEDIEvents Signature;

        public DiadocExportContractWorker(DiadocJobSettings sett)
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
                foreach (Contract document in Documents)
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
            Documents = settings.generalSettings.DbContext.Contracts
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
                    DiadocCommon.AddSignaturesAndEDIEvents(settings, "Отправлен контрагенту", DateTime.Now, "", CurrentDocument.Id, 0, "Endocs.Cloud");
                }
                else
                {
                    UploadNonformalizedSignature();
                    if (Signature.Approved)
                    {
                        DiadocCommon.AddSignaturesAndEDIEvents(settings, "Подпись отправлена контрагенту", DateTime.Now, "", CurrentDocument.Id, 0, "Endocs.Cloud");
                    }
                    else
                    {
                        DiadocCommon.AddSignaturesAndEDIEvents(settings, "Отказ отправлен контрагенту", DateTime.Now, "", CurrentDocument.Id, 0, "Endocs.Cloud");
                    }
                }
                CurrentDocument.EdiNeedExport = false;
                CurrentDocument.EDIState = "Выгружено в Диадок";
                settings.generalSettings.DbContext.SaveChanges();
                settings.generalSettings.LogService.AddInfo("Обновлен договор " + CurrentDocument.Name + " Ид: " + CurrentDocument.Id, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
            }
            catch (Exception e)
            {
                settings.generalSettings.LogService.AddError("Произошла ошибка во время выгрузки договора " + CurrentDocument.Name + " Ид: " + CurrentDocument.Id + " : " + e.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);

                CurrentDocument.EDIState = "Ошибка отправки";
                CurrentDocument.Modified = DateTime.Now;
                CurrentDocument.EDIProcessed = true;
                CurrentDocument.EdiNeedExport = false;
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

            var files = settings.generalSettings.DbContext.Files.Where(x => x.ContractId == CurrentDocument.Id);
            DocFile file = null;
            foreach (var f in files)
            {
                if (settings.generalSettings.DbContext.SignaturesAndEDIEvents.Where(x => x.FileID == f.Id).Count() > 0)
                {
                    file = f;
                    break;
                }
            }
            if (file == null)
                throw new Exception("Не найден подписанный файл у договора");

            bb = st.Connection.GetBox(BoxID);
            mess = new MessageToPost();
            mess.FromBoxId = BoxID;
            var Contractor = settings.generalSettings.DbContext.Contractors.Where(x => x.Id == CurrentDocument.ContractorId).FirstOrDefault();
            var CTCode = DiadocCommon.GetContractorID(settings, Contractor);
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

            //Если будет отдельно ДС
            /*if ("SupplementaryAgreement" ??)
            {
                att.Metadata.Add(new MetadataItem("ContractDocumentNumber", doc.DocumentNumber));
                att.Metadata.Add(new MetadataItem("ContractDocumentDate", doc.DocumentDate.Value.ToString("dd.MM.yyyy")));
            }*/

            att.TypeNamedId = "Contract";
            if (CurrentDocument.Amount.HasValue)
                att.Metadata.Add(new MetadataItem("ContractPrice", CurrentDocument.Amount.Value.ToString()));

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

            var files = settings.generalSettings.DbContext.Files.Where(x => x.ContractId == CurrentDocument.Id);
            SignaturesAndEDIEvents signBase = null;
            DocFile file = null;
            foreach (var f in files)
            {
                var signs = settings.generalSettings.DbContext.SignaturesAndEDIEvents.Where(x => x.FileID == f.Id);
                if (signs.Count() > 0)
                {
                    signBase = signs.FirstOrDefault();
                    file = f;
                    break;
                }
            }
            if (file == null)
                throw new Exception("Не найден подписанный файл у договора");

            Signature = settings.generalSettings.DbContext.SignaturesAndEDIEvents.Where(x => x.FileID == file.Id).FirstOrDefault();
            if (Signature.Approved)
            {
                DocumentSignature sign = new DocumentSignature();
                sign.ParentEntityId = EntityID;
                sign.Signature = GetSignatureSign(file);
                mess.Signatures.Add(sign);
                var msg = st.Connection.PostMessagePatch(st.Token, mess);
                SetPatchFieldValues(msg);
            }
            else
            {
                SignedContent content = new SignedContent();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(signBase.Comment);
                content.Content = data;
                content.Signature = GetSignatureSign(file);
                RequestedSignatureRejection rej = new RequestedSignatureRejection();
                rej.ParentEntityId = EntityID;
                rej.SignedContent = content;
                mess.RequestedSignatureRejections.Add(rej);
                var msg = st.Connection.PostMessagePatch(st.Token, mess);
                SetPatchFieldValues(msg);
            }
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
                    Document di = settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, MessageId, EntityId);
                    CurrentDocument.EDIId = DocID;
                    CurrentDocument.Modified = DateTime.Now;
                }
            }
        }

        private void SetPatchFieldValues(MessagePatch msg)
        {
            CurrentDocument.Modified = DateTime.Now;
        }
    }
}
