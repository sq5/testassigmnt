using System;
using System.Linq;
using System.Data;
using Diadoc.Api;
using Diadoc.Api.Proto.Events;
using Diadoc.Api.Proto.Documents;
using ARCHIVE.COMMON.Entities;
using System.Text;
using CloudArchive.Services.EDI.EnsolDiadoc;
using System.Globalization;
using AutoMapper.Configuration.Conventions;

namespace CloudArchive.ScheduledTasks
{
    public class DiadocDocumentLoader
    {
        private DiadocJobSettings settings { get; set; }


        private BoxEvent BoxEvent;
        private Message msg;
        private Entity en;
        private string DocID;
        private readonly string BoxID;
        private readonly int OrgID;
        private bool IsIncoming;
        Contract contract;
        Metadata meta;

        private string State;
        private bool Final;
        private string Comment;

        public DiadocDocumentLoader(DiadocJobSettings Settings)
        {
            settings = Settings;
            BoxID = settings.ediSettings.ConnectionInfo.EDIUserID.Replace("-", "");
            if (!settings.generalSettings.DbContext.Organizations.Where(x => x.ClientId == settings.ediSettings.ConnectionInfo.ClientID &&
             x.INN == settings.ediSettings.ConnectionInfo.OrganizationINN && x.KPP == settings.ediSettings.ConnectionInfo.OrganizationKPP).Any())
            {
                ARCHIVE.COMMON.Entities.Organization org = new ARCHIVE.COMMON.Entities.Organization();
                org.Name = settings.ediSettings.ConnectionInfo.OrganizationName;
                org.ClientId = settings.ediSettings.ConnectionInfo.ClientID;
                org.INN = settings.ediSettings.ConnectionInfo.OrganizationINN;
                org.KPP = settings.ediSettings.ConnectionInfo.OrganizationKPP;
                settings.generalSettings.DbContext.Organizations.Add(org);
                settings.generalSettings.DbContext.SaveChanges();
            }
            OrgID = settings.generalSettings.DbContext.Organizations.Where(x => x.ClientId == settings.ediSettings.ConnectionInfo.ClientID &&
           x.INN == settings.ediSettings.ConnectionInfo.OrganizationINN && x.KPP == settings.ediSettings.ConnectionInfo.OrganizationKPP).FirstOrDefault().Id;
        }

        public bool Load(BoxEvent boxEvent)
        {
            bool wasLoaded = false;
            msg = null;
            DocID = "";
            BoxEvent = boxEvent;
            if (boxEvent.Entities == null)
                return false;
            for (int i = 0; i < BoxEvent.Entities.Count; i++)
            {
                en = BoxEvent.EntitiesList.Item(i) as Entity;
                if (en.EntityType == EntityType.Attachment && en.DocumentInfo != null)
                {
                    string atype = en.AttachmentType.ToString();
                    if ((atype == "Contract" || atype == "SupplementaryAgreement") && settings.ediSettings.ConnectionInfo.LoadContracts)
                    {
                        wasLoaded = ProcessContract();
                    }
                    else if (settings.ediSettings.ConnectionInfo.LoadBuhDocs)
                    {
                        if (string.IsNullOrEmpty(en.DocumentInfo.PacketId) && string.IsNullOrEmpty(en.PacketId))
                        {
                            if ((
                                en.AttachmentType == AttachmentType.Invoice || en.AttachmentType == AttachmentType.InvoiceCorrection ||
                                en.AttachmentType == AttachmentType.InvoiceRevision || en.AttachmentType == AttachmentType.InvoiceCorrectionRevision ||
                                ((
                                en.AttachmentType == AttachmentType.UniversalCorrectionDocument || en.AttachmentType == AttachmentType.UniversalCorrectionDocumentRevision ||
                                en.AttachmentType == AttachmentType.UniversalTransferDocument || en.AttachmentType == AttachmentType.UniversalTransferDocumentRevision
                                ) &&
                                (
                                en.DocumentInfo.Function.ToLower() == "счф" || en.DocumentInfo.Function.ToLower() == "ксчф"
                                ))))
                            {
                                wasLoaded = ProcessMetadataInvoice();
                                return wasLoaded;
                            }
                            if (atype == "ProformaInvoice")
                            {
                                wasLoaded = ProcessMetadataProformaInvoice();
                                return wasLoaded;
                            }
                        }
                        if (
                            atype == "AcceptanceCertificate" || atype == "Torg12" ||
                            en.AttachmentType == AttachmentType.XmlAcceptanceCertificate || en.AttachmentType == AttachmentType.XmlTorg12 ||
                            en.AttachmentType == AttachmentType.UniversalCorrectionDocument || en.AttachmentType == AttachmentType.UniversalTransferDocument ||
                            en.AttachmentType == AttachmentType.UniversalTransferDocumentRevision || en.AttachmentType == AttachmentType.UniversalCorrectionDocumentRevision
                            )
                        {
                            wasLoaded = ProcessMetadata();
                        }
                        else if (atype == "ReconciliationAct")
                        {
                            wasLoaded = ProcessMetadataByType(10);
                        }
                    }
                }
            }
            return wasLoaded;
        }

        private void GetMessage()
        {
            try
            {
                msg = settings.ediSettings.Connection.GetMessage(settings.ediSettings.Token, BoxID, BoxEvent.MessageId);
            }
            catch
            {
                msg = null;
            }
        }
        public static string FindComment(DiadocApi Connection, string Token, string BoxID, Message msg, string ParentId)
        {
            foreach (Entity e in msg.Entities)
                if (e.AttachmentType == AttachmentType.AttachmentComment && e.ParentEntityId == ParentId)
                {
                    byte[] data = Connection.GetEntityContent(Token, BoxID, msg.MessageId, e.EntityId);
                    return Encoding.UTF8.GetString(data);
                }
            return "";
        }

        #region Contract
        private bool ProcessContract()
        {
            GetMessage();
            if (msg == null || msg.IsDeleted || msg.IsDraft || msg.MessageType == MessageType.Template)
                return false;
            if (settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, msg.MessageId, en.EntityId).IsDeleted)
                return false;
            string MessageId = msg.MessageId;
            string EntityId = en.EntityId;
            IsIncoming = msg.ToBoxId.ToLower() == BoxID.ToLower();
            DocID = MessageId + EntityId;
            var existingContracts = settings.generalSettings.DbContext.Contracts.Where(x => x.EDIId == DocID && x.OrganizationId == OrgID);
            if (!existingContracts.Any())
            {
                contract = new Contract();
                SetMainFieldValuesContract();
                settings.generalSettings.DbContext.Contracts.Add(contract);
                settings.generalSettings.DbContext.SaveChanges();
                UpdateFilesContract();
                string diadloadmess = IsIncoming ? "Отправлен контрагентом" : "Отправлен контрагенту";
                DiadocCommon.AddSignaturesAndEDIEvents(settings, diadloadmess, en.CreationTime, Comment, contract.Id, 0);
                DiadocCommon.AddSignaturesAndEDIEvents(settings, "Загружен в Endocs.Cloud", DateTime.Now, "", contract.Id, 0, "Endocs.Cloud");

                if (Final)
                    DiadocCommon.AddSignaturesAndEDIEvents(settings, State, DateTime.Now, "", contract.Id, 0);
                settings.generalSettings.DbContext.SaveChanges();
                settings.GeneralSettings.LogService.AddInfo("Загружен договор " + contract.Name, settings.GeneralSettings.ServiceName, settings.EdiSettings.ConnectionInfo.ClientID);

            }
            return true;
        }

        private void SetMainFieldValuesContract()
        {
            byte[] data = settings.ediSettings.Connection.GetEntityContent(settings.ediSettings.Token, BoxID, msg.MessageId, en.EntityId);
            string MessageId = msg.MessageId;
            string EntityId = en.EntityId;
            DateTime CreatedDate = DateTime.TryParse(en.DocumentInfo.DocumentDate, out CreatedDate) ? CreatedDate : DateTime.Today;
            Document di = settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, MessageId, EntityId);

            State = "";
            Final = DiadocCommon.CheckIfStatusIsFinal(di, IsIncoming, out State);
            contract.EDIProcessed = Final;
            contract.EDIState = State;
            contract.EDIId = DocID;
            contract.EDIProvider = "Диадок";
            contract.ClientId = settings.ediSettings.ConnectionInfo.ClientID;
            contract.OrganizationId = OrgID;
            contract.DocNumber = en.DocumentInfo.DocumentNumber;
            contract.DocDate = CreatedDate.AddHours(12);
            contract.Deleted = false;
            contract.Modified = DateTime.Now;
            contract.Created = DateTime.Now;
            contract.ModifiedBy = "ExternalSystem";
            contract.CreatedBy = "ExternalSystem";
            contract.DocTypeId = 4;
            contract.EDIIsIncoming = IsIncoming;
            contract.Name = en.DocumentInfo.Title;
            Comment = FindComment(settings.ediSettings.Connection, settings.ediSettings.Token, BoxID, msg, EntityId);
            contract.Comment = Comment;
            contract.State = "Новый";
            SetContractorFieldContract();
            ParseSummFiedsContract(en, data);
        }

        public void UpdateFilesContract()
        {
            string MessageId = msg.MessageId;
            string EntityId = en.EntityId;
            foreach (DiadocFileInfo file in DiadocCommon.DownloadFiles(MessageId, EntityId, BoxID, en.DocumentInfo, settings, true))
            {
                DiadocCommon.AddorUpdateContractDocument(settings, file, contract);
            }
        }

        public void ParseSummFiedsContract(Entity en, byte[] data)
        {
            XMLMetadata parser = new XMLMetadata(settings.ediSettings.Connection, settings.ediSettings.Token, en, BoxID);
            try
            {
                parser.ParseFieds(data);
                contract.Amount = double.Parse(parser.Summ);
                contract.AmountWOVAT = double.Parse(parser.SummWOVAT);
                contract.VAT = double.Parse(parser.VatSumm);
                contract.Currency = DiadocMapping.GetCurrencyShortName(parser.Currency);
            }
            catch (Exception e)
            {
                settings.generalSettings.LogService.AddError("Произошла ошибка во время парсинга метаданных MessageId: " + msg.MessageId + " EntityId: " + en.EntityId + " : " + e.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
            }
        }

        private void SetContractorFieldContract()
        {
            string Counteragent = msg.ToBoxId;
            if (msg.ToBoxId.ToLower() == BoxID.ToLower())
            {
                Counteragent = msg.FromBoxId;
            }
            Contractor ct = DiadocCommon.GetContractor(settings, Counteragent);
            contract.ContractorId = ct.Id;
        }
        #endregion

        #region Metadata
        private bool ProcessMetadata()
        {
            GetMessage();
            if (msg == null || msg.IsDeleted || msg.IsDraft || msg.MessageType == MessageType.Template)
                return false;
            if (settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, msg.MessageId, en.EntityId).IsDeleted)
                return false;
            string MessageId = msg.MessageId;
            string EntityId = en.EntityId;
            IsIncoming = msg.ToBoxId.ToLower() == BoxID.ToLower();
            DocID = MessageId + EntityId;
            var existingDocss = settings.generalSettings.DbContext.Metadatas.Where(x => x.EDIId == DocID && x.OrganizationId == OrgID);
            if (!existingDocss.Any())
            {
                meta = new Metadata();
                var DocTypeId = IsIncoming ? 1 : 2;
                SetMainFieldValuesMetadata(DocTypeId);
                settings.generalSettings.DbContext.Metadatas.Add(meta);
                settings.generalSettings.DbContext.SaveChanges();
                UpdateFilesMetadata();
                string diadloadmess = IsIncoming ? "Отправлен контрагентом" : "Отправлен контрагенту";
                DiadocCommon.AddSignaturesAndEDIEvents(settings, diadloadmess, en.CreationTime, Comment, 0, meta.Id);
                DiadocCommon.AddSignaturesAndEDIEvents(settings, "Загружен в Endocs.Cloud", DateTime.Now, "", 0, meta.Id, "Endocs.Cloud");

                if (Final)
                    DiadocCommon.AddSignaturesAndEDIEvents(settings, State, DateTime.Now, "", 0, meta.Id);
                settings.generalSettings.DbContext.SaveChanges();
                settings.GeneralSettings.LogService.AddInfo("Загружен документ " + meta.Id, settings.GeneralSettings.ServiceName, settings.EdiSettings.ConnectionInfo.ClientID);

            }
            return true;
        }
        private bool ProcessMetadataByType(int DocTypeId)
        {
            GetMessage();
            if (msg == null || msg.IsDeleted || msg.IsDraft || msg.MessageType == MessageType.Template)
                return false;
            if (settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, msg.MessageId, en.EntityId).IsDeleted)
                return false;
            string MessageId = msg.MessageId;
            string EntityId = en.EntityId;
            IsIncoming = msg.ToBoxId.ToLower() == BoxID.ToLower();
            DocID = MessageId + EntityId;
            var existingDocss = settings.generalSettings.DbContext.Metadatas.Where(x => x.EDIId == DocID && x.OrganizationId == OrgID);
            if (!existingDocss.Any())
            {
                meta = new Metadata();
                SetMainFieldValuesMetadata(DocTypeId);
                settings.generalSettings.DbContext.Metadatas.Add(meta);
                settings.generalSettings.DbContext.SaveChanges();
                UpdateFilesMetadata();
                string diadloadmess = IsIncoming ? "Отправлен контрагентом" : "Отправлен контрагенту";
                DiadocCommon.AddSignaturesAndEDIEvents(settings, diadloadmess, en.CreationTime, Comment, 0, meta.Id);
                DiadocCommon.AddSignaturesAndEDIEvents(settings, "Загружен в Endocs.Cloud", DateTime.Now, "", 0, meta.Id, "Endocs.Cloud");

                if (Final)
                    DiadocCommon.AddSignaturesAndEDIEvents(settings, State, DateTime.Now, "", 0, meta.Id);
                settings.generalSettings.DbContext.SaveChanges();
                settings.GeneralSettings.LogService.AddInfo("Загружен документ " + meta.Id, settings.GeneralSettings.ServiceName, settings.EdiSettings.ConnectionInfo.ClientID);

            }
            return true;
        }

        private bool ProcessMetadataInvoice()
        {
            GetMessage();
            if (msg == null || msg.IsDeleted || msg.IsDraft || msg.MessageType == MessageType.Template)
                return false;
            if (settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, msg.MessageId, en.EntityId).IsDeleted)
                return false;
            string MessageId = msg.MessageId;
            string EntityId = en.EntityId;
            IsIncoming = msg.ToBoxId.ToLower() == BoxID.ToLower();
            DocID = MessageId + EntityId;
            var existingDocss = settings.generalSettings.DbContext.Metadatas.Where(x => x.EDIId == DocID && x.OrganizationId == OrgID);
            if (!existingDocss.Any())
            {
                meta = new Metadata();
                var DocTypeId = IsIncoming ? 15 : 5;
                SetMainFieldValuesMetadata(DocTypeId);
                settings.generalSettings.DbContext.Metadatas.Add(meta);
                settings.generalSettings.DbContext.SaveChanges();
                UpdateFilesMetadata();
                string diadloadmess = IsIncoming ? "Отправлен контрагентом" : "Отправлен контрагенту";
                DiadocCommon.AddSignaturesAndEDIEvents(settings, diadloadmess, en.CreationTime, Comment, 0, meta.Id);
                DiadocCommon.AddSignaturesAndEDIEvents(settings, "Загружен в Endocs.Cloud", DateTime.Now, "", 0, meta.Id, "Endocs.Cloud");

                if (Final)
                    DiadocCommon.AddSignaturesAndEDIEvents(settings, State, DateTime.Now, "", 0, meta.Id);
                settings.generalSettings.DbContext.SaveChanges();
                settings.GeneralSettings.LogService.AddInfo("Загружен счет-фактура " + meta.Id, settings.GeneralSettings.ServiceName, settings.EdiSettings.ConnectionInfo.ClientID);

            }
            return true;
        }

        private bool ProcessMetadataProformaInvoice()
        {
            GetMessage();
            if (msg == null || msg.IsDeleted || msg.IsDraft || msg.MessageType == MessageType.Template)
                return false;
            if (settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, msg.MessageId, en.EntityId).IsDeleted)
                return false;
            string MessageId = msg.MessageId;
            string EntityId = en.EntityId;
            IsIncoming = msg.ToBoxId.ToLower() == BoxID.ToLower();
            DocID = MessageId + EntityId;
            var existingDocss = settings.generalSettings.DbContext.Metadatas.Where(x => x.EDIId == DocID && x.OrganizationId == OrgID);
            if (!existingDocss.Any())
            {
                meta = new Metadata();
                var DocTypeId = IsIncoming ? 14 : 6;
                SetMainFieldValuesMetadata(DocTypeId);
                settings.generalSettings.DbContext.Metadatas.Add(meta);
                settings.generalSettings.DbContext.SaveChanges();
                UpdateFilesMetadata();
                string diadloadmess = IsIncoming ? "Отправлен контрагентом" : "Отправлен контрагенту";
                DiadocCommon.AddSignaturesAndEDIEvents(settings, diadloadmess, en.CreationTime, Comment, 0, meta.Id);
                DiadocCommon.AddSignaturesAndEDIEvents(settings, "Загружен в Endocs.Cloud", DateTime.Now, "", 0, meta.Id, "Endocs.Cloud");

                if (Final)
                    DiadocCommon.AddSignaturesAndEDIEvents(settings, State, DateTime.Now, "", 0, meta.Id);
                settings.generalSettings.DbContext.SaveChanges();
                settings.GeneralSettings.LogService.AddInfo("Загружен счет " + meta.Id, settings.GeneralSettings.ServiceName, settings.EdiSettings.ConnectionInfo.ClientID);

            }
            return true;
        }

        private void SetMainFieldValuesMetadata(int Doctype)
        {
            byte[] data = settings.ediSettings.Connection.GetEntityContent(settings.ediSettings.Token, BoxID, msg.MessageId, en.EntityId);
            string MessageId = msg.MessageId;
            string EntityId = en.EntityId;
            DateTime CreatedDate = DateTime.TryParse(en.DocumentInfo.DocumentDate, out CreatedDate) ? CreatedDate : DateTime.Today;
            Document di = settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, MessageId, EntityId);

            State = "";
            Final = DiadocCommon.CheckIfStatusIsFinal(di, IsIncoming, out State);
            meta.EDIProcessed = Final;
            meta.EDIState = State;
            meta.EDIId = DocID;
            meta.EDIProvider = "Диадок";
            meta.ClientId = settings.ediSettings.ConnectionInfo.ClientID;
            meta.OrganizationId = OrgID;
            meta.DocNumber = en.DocumentInfo.DocumentNumber;
            meta.DocDate = CreatedDate.AddHours(12);
            meta.Deleted = false;
            meta.Modified = DateTime.Now;
            meta.Created = DateTime.Now;
            meta.ModifiedBy = "ExternalSystem";
            meta.CreatedBy = "ExternalSystem";
            meta.DocTypeId = Doctype;
            meta.EDIIsIncoming = IsIncoming;
            Comment = FindComment(settings.ediSettings.Connection, settings.ediSettings.Token, BoxID, msg, EntityId);
            meta.Comment = Comment;
            meta.State = "Новый";

            if (en.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.Invoice ||
                en.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.InvoiceCorrection ||
                en.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.InvoiceCorrectionRevision ||
                en.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.InvoiceRevision)
            {
                if (!string.IsNullOrEmpty(en.DocumentInfo.DocumentDate))
                    meta.DocDateInvoice = DateTime.ParseExact(en.DocumentInfo.DocumentDate, "dd.MM.yyyy", CultureInfo.InvariantCulture).AddHours(12);
                if (en.DocumentInfo.DocumentNumber != null)
                    meta.DocNumInvoice = en.DocumentInfo.DocumentNumber;
            }
            else if (en.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.ProformaInvoice)
            {
                if (!string.IsNullOrEmpty(en.DocumentInfo.DocumentDate))
                    meta.DocDateTaxInvoice = DateTime.ParseExact(en.DocumentInfo.DocumentDate, "dd.MM.yyyy", CultureInfo.InvariantCulture).AddHours(12);
                if (en.DocumentInfo.DocumentNumber != null)
                    meta.DocNumTaxInvoice = en.DocumentInfo.DocumentNumber;
                if (!string.IsNullOrEmpty(en.DocumentInfo.ProformaInvoiceMetadata.Total))
                    meta.AmountToPay = double.Parse(en.DocumentInfo.ProformaInvoiceMetadata.Total, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(en.DocumentInfo.PacketId))
            {
                Diadoc.Api.Proto.Docflow.GetDocflowsByPacketIdRequest request = new Diadoc.Api.Proto.Docflow.GetDocflowsByPacketIdRequest();
                request.Count = 5;
                request.PacketId = en.DocumentInfo.PacketId;
                var result = settings.ediSettings.Connection.GetDocflowsByPacketId(settings.ediSettings.Token, BoxID, request);
                foreach (var packetDoc in result.Documents)
                {
                    if (packetDoc.Document.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.Invoice ||
                        packetDoc.Document.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.InvoiceCorrection ||
                        packetDoc.Document.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.InvoiceCorrectionRevision ||
                        packetDoc.Document.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.InvoiceRevision)
                    {
                        var dnn = packetDoc.Document.DocumentInfo.DocumentDateAndNumber;
                        if (dnn.DocumentDate != null)
                            meta.DocDateInvoice = DateTime.ParseExact(dnn.DocumentDate, "dd.MM.yyyy", CultureInfo.InvariantCulture).AddHours(12);
                        if (dnn.DocumentNumber != null)
                            meta.DocNumInvoice = dnn.DocumentNumber;
                    }
                    else if (packetDoc.Document.DocumentInfo.DocumentType == Diadoc.Api.Proto.DocumentType.ProformaInvoice)
                    {
                        var dnn = packetDoc.Document.DocumentInfo.DocumentDateAndNumber;
                        if (dnn.DocumentDate != null)
                            meta.DocDateTaxInvoice = DateTime.ParseExact(dnn.DocumentDate, "dd.MM.yyyy", CultureInfo.InvariantCulture).AddHours(12);
                        if (dnn.DocumentNumber != null)
                            meta.DocNumTaxInvoice = dnn.DocumentNumber;
                        var DocId = packetDoc.Document.DocumentId;
                        Document tmp = settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, DocId.MessageId, DocId.EntityId);
                        if (!string.IsNullOrEmpty(tmp.ProformaInvoiceMetadata.Total))
                            meta.AmountToPay = double.Parse(tmp.ProformaInvoiceMetadata.Total, CultureInfo.InvariantCulture);
                    }
                }
            }
            SetContractorFieldMetadata();
            ParseSummFiedsMetadata(en, data);
        }

        public void UpdateFilesMetadata()
        {
            string MessageId = msg.MessageId;
            string EntityId = en.EntityId;
            foreach (DiadocFileInfo file in DiadocCommon.DownloadPackageFiles(MessageId, EntityId, BoxID, IsIncoming, en.DocumentInfo, settings))
            {
                DiadocCommon.AddorUpdateMetadataDocument(settings, file, meta);
            }
        }

        public void ParseSummFiedsMetadata(Entity en, byte[] data)
        {
            XMLMetadata parser = new XMLMetadata(settings.ediSettings.Connection, settings.ediSettings.Token, en, BoxID);
            try
            {
                parser.ParseFieds(data);
                meta.Amount = double.Parse(parser.Summ, CultureInfo.InvariantCulture);
                meta.AmountWOVAT = double.Parse(parser.SummWOVAT, CultureInfo.InvariantCulture);
                meta.VAT = double.Parse(parser.VatSumm, CultureInfo.InvariantCulture);
                meta.Currency = DiadocMapping.GetCurrencyShortName(parser.Currency);
                meta.TablePart = parser.TablePart;
            }
            catch (Exception e)
            {
                settings.generalSettings.LogService.AddError("Произошла ошибка во время парсинга метаданных MessageId: " + msg.MessageId + " EntityId: " + en.EntityId + " : " + e.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
            }
        }

        private void SetContractorFieldMetadata()
        {
            string Counteragent = msg.ToBoxId;
            if (msg.ToBoxId.ToLower() == BoxID.ToLower())
            {
                Counteragent = msg.FromBoxId;
            }
            Contractor ct = DiadocCommon.GetContractor(settings, Counteragent);
            meta.ContractorId = ct.Id;
        }
        #endregion
    }
}
