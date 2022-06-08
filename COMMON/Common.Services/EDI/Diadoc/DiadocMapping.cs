// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Diadoc.Api.Proto.Documents;
using System.Collections.Generic;
using System.Linq;

namespace CloudArchive.Services.EDI.EnsolDiadoc
{
    public static class DiadocMapping
    {
        public static string GetCurrencyShortName(string Code)
        {
            string result = "";
            if (Currencies.Where(x => x.Code == Code).Any())
                result = Currencies.Where(x => x.Code == Code).FirstOrDefault().ShortName;
            else
                result = Currencies.Where(x => x.Code == "643").FirstOrDefault().ShortName;
            return result;
        }

        public static string GetDocumentStatus(Document document, bool IsIncoming)
        {
            if (document == null)
                return "UndefinedDocument";
            string DocStatus = "UndefinedDocument";
            DocStatus = document.RecipientResponseStatus.ToString();
            try
            {
                int invStatus = 0;
                if (document.UniversalTransferDocumentMetadata != null)
                    invStatus = document.UniversalTransferDocumentMetadata.InvoiceAmendmentFlags;
                else if (document.UniversalCorrectionDocumentMetadata != null)
                    invStatus = document.UniversalCorrectionDocumentMetadata.InvoiceAmendmentFlags;
                else if (document.UniversalCorrectionDocumentRevisionMetadata != null)
                    invStatus = document.UniversalCorrectionDocumentRevisionMetadata.InvoiceAmendmentFlags;
                else if (document.UniversalTransferDocumentRevisionMetadata != null)
                    invStatus = document.UniversalTransferDocumentRevisionMetadata.InvoiceAmendmentFlags;
                else if (document.InvoiceMetadata != null)
                    invStatus = document.InvoiceMetadata.InvoiceAmendmentFlags;
                else if (document.InvoiceCorrectionMetadata != null)
                    invStatus = document.InvoiceCorrectionMetadata.InvoiceAmendmentFlags;
                else if (document.InvoiceRevisionMetadata != null)
                    invStatus = document.InvoiceRevisionMetadata.InvoiceAmendmentFlags;
                else if (document.InvoiceCorrectionRevisionMetadata != null)
                    invStatus = document.InvoiceCorrectionRevisionMetadata.InvoiceAmendmentFlags;
                if (invStatus != 0)
                {
                    switch (invStatus)
                    {
                        case 1:
                            {
                                DocStatus = "AmendmentRequested";
                                break;
                            }
                        case 2:
                        case 3:
                            {
                                DocStatus = "Revised";
                                break;
                            }
                        case 4:
                        case 5:
                            {
                                DocStatus = "Corrected";
                                break;
                            }
                    }
                }
            }
            catch { }
            try
            {
                string AnnStatus = document.RevocationStatus.ToString();
                if (AnnStatus != "RevocationStatusNone" && AnnStatus != "UnknownRevocationStatus")
                    DocStatus = AnnStatus;
            }
            catch { }
            if (IsIncoming)
                DocStatus = "Incoming" + DocStatus;
            else
                DocStatus = "Outgoing" + DocStatus;
            return DocStatus;
        }

        public static string GetDocumentStatusTranslated(string Status)
        {
            string DocStatus = "Неизвестный статус";
            if (DocStatuses.ContainsKey(Status))
                DocStatus = DocStatuses[Status];
            return DocStatus;
        }

        public static Dictionary<string, string> DocTypes
        {
            get
            {
                Dictionary<string, string> _doctypes = new Dictionary<string, string>();
                _doctypes.Add("AcceptanceCertificate", "Акт выполненных работ");
                _doctypes.Add("CertificateRegistry", "Реестр сертификатов");
                _doctypes.Add("Contract", "Договор");
                _doctypes.Add("Invoice", "Счет-фактура");
                _doctypes.Add("InvoiceCorrection", "Счет-фактура");
                _doctypes.Add("InvoiceCorrectionRevision", "Товарная накладная");
                _doctypes.Add("InvoiceRevision", "Товарная накладная");
                _doctypes.Add("Nonformalized", "Нерубрицированный");
                _doctypes.Add("PriceList", "Ценовой лист");
                _doctypes.Add("PriceListAgreement", "Протокол согласования цены");
                _doctypes.Add("ProformaInvoice", "Счет");
                _doctypes.Add("ReconciliationAct", "Акт сверки");
                _doctypes.Add("SupplementaryAgreement", "Дополнительное соглашение");
                _doctypes.Add("Torg12", "Товарная накладная");
                _doctypes.Add("Torg13", "Накладная ТОРГ-13");
                _doctypes.Add("UniversalCorrectionDocument", "Корректировка УПД");
                _doctypes.Add("UniversalCorrectionDocumentRevision", "Исправление корректировки УПД");
                _doctypes.Add("UniversalTransferDocument", "УПД");
                _doctypes.Add("UniversalTransferDocumentRevision", "Исправление УПД");
                _doctypes.Add("XmlAcceptanceCertificate", "Акт выполненных работ");
                _doctypes.Add("XmlTorg12", "Товарная накладная");
                return _doctypes;
            }
        }

        public static Dictionary<string, string> DocStatuses
        {
            get
            {
                Dictionary<string, string> _docstatuses = new Dictionary<string, string>();
                _docstatuses.Add("OutgoingInvalidRecipientSignature", "Ошибка в подписи");
                _docstatuses.Add("OutgoingRecipientResponseStatusNotAcceptable", "Документооборот завершен");
                _docstatuses.Add("OutgoingRecipientSignatureRequestRejected", "Отказ в подписи");
                _docstatuses.Add("OutgoingRequestsMyRevocation", "Запрос на аннулирование");
                _docstatuses.Add("OutgoingRevocationAccepted", "Документ аннулирован");
                _docstatuses.Add("OutgoingRevocationIsRequestedByMe", "Запрос на аннулирование");
                _docstatuses.Add("OutgoingRevocationRejected", "Отказ от аннулирования");
                _docstatuses.Add("OutgoingRevocationStatusNone", "Запрос на аннулирование");
                _docstatuses.Add("OutgoingUnknownRevocationStatus", "Неизвестный статус аннулирования");
                _docstatuses.Add("OutgoingWaitingForRecipientSignature", "На подписании у контрагента");
                _docstatuses.Add("OutgoingWithRecipientSignature", "Подписан контрагентом");
                _docstatuses.Add("OutgoingAmendmentRequested", "Ожидается уточнение");
                _docstatuses.Add("OutgoingCorrected", "Откорректирован");
                _docstatuses.Add("OutgoingRevised", "Исправлен");

                _docstatuses.Add("IncomingInvalidRecipientSignature", "Ошибка в подписи");
                _docstatuses.Add("IncomingRecipientResponseStatusNotAcceptable", "Документооборот завершен");
                _docstatuses.Add("IncomingRecipientSignatureRequestRejected", "Отказ в подписи");
                _docstatuses.Add("IncomingRequestsMyRevocation", "Запрос на аннулирование");
                _docstatuses.Add("IncomingRevocationAccepted", "Документ аннулирован");
                _docstatuses.Add("IncomingRevocationIsRequestedByMe", "Запрос на аннулирование");
                _docstatuses.Add("IncomingRevocationRejected", "Отказ от аннулирования");
                _docstatuses.Add("IncomingRevocationStatusNone", "Запрос на аннулирование");
                _docstatuses.Add("IncomingUnknownRevocationStatus", "Неизвестный статус аннулирования");
                _docstatuses.Add("IncomingWaitingForRecipientSignature", "Требуется подпись");
                _docstatuses.Add("IncomingWithRecipientSignature", "Документ подписан");
                _docstatuses.Add("IncomingAmendmentRequested", "Ожидается уточнение");
                _docstatuses.Add("IncomingCorrected", "Откорректирован");
                _docstatuses.Add("IncomingRevised", "Исправлен");
                return _docstatuses;
            }
        }


        public static List<Currency> Currencies
        {
            get
            {
                List<Currency> _currencies = new List<Currency>();
                _currencies.Add(new Currency("Доллар США", "USD", "840"));
                _currencies.Add(new Currency("Евро", "EUR", "978"));
                _currencies.Add(new Currency("Фунт стерлингов Великобритании", "GBP", "826"));
                _currencies.Add(new Currency("Японская йена", "JPY", "392"));
                _currencies.Add(new Currency("Швейцарский франк", "CHF", "756"));
                _currencies.Add(new Currency("Китайский юань женьминьби", "CNY", "156"));
                _currencies.Add(new Currency("Российский рубль", "RUB", "643"));
                _currencies.Add(new Currency("Украинская гривна", "UAH", "980"));
                return _currencies;
            }
        }
    }

    public class Currency
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Code { get; set; }

        public Currency(string name, string shortname, string code)
        {
            Name = name;
            ShortName = shortname;
            Code = code;
        }
    }
}
