using System;
using System.Linq;
using System.Text;
using Diadoc.Api;
using Diadoc.Api.Proto.Events;
using Diadoc.Api.Proto.Invoicing;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using Diadoc.Api.DataXml.Utd820.Hyphens;
using Diadoc.Api.DataXml;
using BackgroudServices.ScheduledTasks.OCR.Common;
using Newtonsoft.Json;
using System.Globalization;

namespace CloudArchive.ScheduledTasks
{
    [Serializable]
    public class XMLMetadata
    {
        [XmlIgnore]
        public string Summ
        {
            get { return string.IsNullOrEmpty(summ) ? "0" : summ.Replace(",", "."); }
        }
        [XmlIgnore]
        public string VatSumm
        {
            get { return string.IsNullOrEmpty(VatAmount) ? "0" : VatAmount.Replace(",", "."); }
        }
        
        [XmlIgnore]
        public string Currency { get; set; } = "";
        [XmlIgnore]
        public string VatAmount { get; set; } = "0";
        [XmlIgnore]
        private string _summWOVAT = "0";
        [XmlIgnore]
        public string SummWOVAT
        {
            get
            {
                return string.IsNullOrEmpty(_summWOVAT) ? "0" : _summWOVAT.Replace(",", ".");
            }
            set { _summWOVAT = value; }
        }
        
        [XmlIgnore]
        public string summ { get; set; } = "0";
        public string TablePart { get; set; } = "";
        [XmlIgnore]
        private List<TableRow> rows = new List<TableRow>();

        [XmlIgnore]
        public string UniversalDocumentType { get; set; } = "";
        [XmlElement(elementName: "Field")]
        public List<InfoRow> Infopol { get; set; } = new List<InfoRow>();

        [XmlElement]
        public Address ConsigneeAdress { get; set; } = new Address();
        [XmlElement]
        public Address SellerAdress { get; set; } = new Address();
        [XmlElement]
        public Address ShipperAdress { get; set; } = new Address();
        [XmlElement]
        public Address BuyerAdress { get; set; } = new Address();
        [XmlIgnore]
        public Operation Operation { get; set; } = new Operation();

        [XmlIgnore]
        private DiadocApi Connection;
        [XmlIgnore]
        private string Token;
        [XmlIgnore]
        private string BoxID;
        [XmlIgnore]
        private Entity en;
        /// <summary>
        /// Конструктор для десериализации из ХМЛ, не используйте
        /// </summary>
        public XMLMetadata()
        {
        }
        /// <summary>
        /// Конструктор для создания метаданных
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="token"></param>
        /// <param name="entity"></param>
        /// <param name="boxID"></param>
        public XMLMetadata(DiadocApi connection, string token, Entity entity, string boxID)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Connection = connection;
            Token = token;
            en = entity;
            BoxID = boxID;
        }

        public void Init(DiadocApi connection, string token, Entity entity, string boxID)
        {
            Connection = connection;
            Token = token;
            en = entity;
            BoxID = boxID;
        }

        public void ParseFieds(byte[] data)
        {
            if (!en.DocumentInfo.FileName.EndsWith(".xml") || !GetFieldsNew(data))
            {
                try
                {
                    UniversalDocumentType = en.DocumentInfo.Function;
                }
                catch { }
                GetFieldsLegacy(data);
            }
            if (!string.IsNullOrEmpty(BuyerAdress.ToString()))
                Infopol.Add(new InfoRow("BuyerAdress", BuyerAdress.ToString()));
            if (!string.IsNullOrEmpty(SellerAdress.ToString()))
                Infopol.Add(new InfoRow("SellerAdress", SellerAdress.ToString()));
            if (!string.IsNullOrEmpty(ConsigneeAdress.ToString()))
                Infopol.Add(new InfoRow("ConsigneeAdress", ConsigneeAdress.ToString()));
            if (!string.IsNullOrEmpty(ShipperAdress.ToString()))
                Infopol.Add(new InfoRow("ShipperAdress", ShipperAdress.ToString()));
            Infopol.Add(new InfoRow("VatAmount", VatAmount.ToString().Replace(",", ".")));
            Infopol.Add(new InfoRow("SummWOVAT", SummWOVAT.ToString().Replace(",", ".")));
            Infopol.Add(new InfoRow("UniversalDocumentType", UniversalDocumentType.ToString()));
            Infopol.Add(new InfoRow("Currency", Currency.ToString()));
            Infopol.Add(new InfoRow("BaseDocumentDate", Operation.BaseDocumentDate));
            Infopol.Add(new InfoRow("BaseDocumentInfo", Operation.BaseDocumentInfo));
            Infopol.Add(new InfoRow("BaseDocumentName", Operation.BaseDocumentName));
            Infopol.Add(new InfoRow("BaseDocumentNumber", Operation.BaseDocumentNumber));
            if (en.DocumentInfo.Origin != null && !string.IsNullOrEmpty(en.DocumentInfo.Origin.MessageId))
            {
                try
                {
                    Infopol.Add(new InfoRow("Origin_MessageID", en.DocumentInfo.Origin.MessageId));
                    Infopol.Add(new InfoRow("Origin_MessageType", en.DocumentInfo.Origin.MessageType.ToString()));
                }
                catch { }
            }
            TablePart = JsonConvert.SerializeObject(rows, Newtonsoft.Json.Formatting.None);
        }

        private bool GetFieldsLegacy(byte[] data)
        {
            bool result = true;
            try
            {
                if (en.AttachmentType == AttachmentType.UniversalTransferDocument || en.AttachmentType == AttachmentType.UniversalTransferDocumentRevision)
                {
                    UniversalTransferDocumentSellerTitleInfo UTD =
                    Connection.ParseUniversalTransferDocumentSellerTitleXml(data, en.Version);
                    ParseUTDOldFormat(UTD);
                }
                else if (en.AttachmentType == AttachmentType.XmlTorg12)
                {
                    if (en.AttachmentVersion == "tovtorg_05_01_04")
                    {
                        TovTorgSellerTitleInfo torg = Connection.ParseTovTorg551SellerTitleXml(data);
                        ParseTovTorgSellerTitleInfoFormat(torg);
                    }
                    else if (en.AttachmentVersion == "utd_05_02_01" || en.AttachmentVersion == "UniversalTransferDocument")
                    {
                        UniversalTransferDocumentSellerTitleInfo UTD =
                        Connection.ParseUniversalTransferDocumentSellerTitleXml(data, en.Version);
                        ParseUTDOldFormat(UTD);
                    }
                    else
                    {
                        Torg12SellerTitleInfo torg =
                    Connection.ParseTorg12SellerTitleXml(data);
                        ParseTorg12SellerTitleInfoFormat(torg);
                    }
                }
                else if (en.AttachmentType == AttachmentType.XmlAcceptanceCertificate)
                {
                    if (en.AttachmentVersion == "act_05_01_02" || en.AttachmentVersion == "act_05_01_01")
                    {
                        AcceptanceCertificateSellerTitleInfo torg =
                        Connection.ParseAcceptanceCertificateSellerTitleXml(data);
                        ParseAcceptanceCertificateFormat(torg);
                    }
                    else if (en.AttachmentVersion == "UniversalTransferDocument")
                    {
                        UniversalTransferDocumentSellerTitleInfo UTD =
                        Connection.ParseUniversalTransferDocumentSellerTitleXml(data, en.Version);
                        ParseUTDOldFormat(UTD);
                    }
                    else
                    {
                        AcceptanceCertificate552SellerTitleInfo torg =
                        Connection.ParseAcceptanceCertificate552SellerTitleXml(data);
                        ParseAcceptanceCertificate552Format(torg);
                    }
                }
                else if (en.AttachmentType == AttachmentType.Invoice || en.AttachmentType == AttachmentType.InvoiceRevision)
                {
                    if (en.AttachmentVersion == "UniversalTransferDocument")
                    {
                        UniversalTransferDocumentSellerTitleInfo UTD =
                        Connection.ParseUniversalTransferDocumentSellerTitleXml(data, en.Version);
                        ParseUTDOldFormat(UTD);
                    }
                    else
                    {
                        InvoiceInfo inv = Connection.ParseInvoiceXml(data);
                        ParseInvoiceInfo(inv);
                    }
                }
                else if (en.AttachmentType == AttachmentType.UniversalCorrectionDocument || en.AttachmentType == AttachmentType.UniversalCorrectionDocumentRevision || en.AttachmentType == AttachmentType.InvoiceCorrection || en.AttachmentType == AttachmentType.InvoiceCorrectionRevision)
                {
                    UniversalCorrectionDocumentSellerTitleInfo UTD =
                    Connection.ParseUniversalCorrectionDocumentSellerTitleXml(data, en.Version);
                    ParseUCDOldFormat(UTD);
                }
                #region Nonformalized
                else if (en.AttachmentType == AttachmentType.Nonformalized || en.AttachmentType == AttachmentType.Contract || en.AttachmentType == AttachmentType.Torg12
                    || en.AttachmentType == AttachmentType.Torg13 || en.AttachmentType == AttachmentType.ProformaInvoice
                    || en.AttachmentType == AttachmentType.AcceptanceCertificate || en.AttachmentType == AttachmentType.Contract || en.AttachmentType == AttachmentType.SupplementaryAgreement)
                {
                    if (en.DocumentInfo.Type == Diadoc.Api.Com.DocumentType.AcceptanceCertificate)
                    {
                        summ = en.DocumentInfo.AcceptanceCertificateMetadata.Total;
                        VatAmount = en.DocumentInfo.AcceptanceCertificateMetadata.Vat;
                    }
                    else if (en.DocumentInfo.Type == Diadoc.Api.Com.DocumentType.Torg12)
                    {
                        summ = en.DocumentInfo.Torg12Metadata.Total;
                        VatAmount = en.DocumentInfo.Torg12Metadata.Vat;
                    }
                    else if (en.DocumentInfo.Type == Diadoc.Api.Com.DocumentType.ProformaInvoice)
                    {
                        summ = en.DocumentInfo.ProformaInvoiceMetadata.Total;
                        VatAmount = en.DocumentInfo.ProformaInvoiceMetadata.Vat;
                    }
                    else if (en.DocumentInfo.Type == Diadoc.Api.Com.DocumentType.Torg13)
                    {
                        summ = en.DocumentInfo.Torg13Metadata.Total;
                        VatAmount = en.DocumentInfo.Torg13Metadata.Vat;
                    }
                    else if (en.DocumentInfo.Type == Diadoc.Api.Com.DocumentType.Contract)
                    {
                        summ = en.DocumentInfo.ContractMetadata.ContractPrice;
                    }
                    else if (en.DocumentInfo.Type == Diadoc.Api.Com.DocumentType.SupplementaryAgreement)
                    {
                        summ = en.DocumentInfo.SupplementaryAgreementMetadata.Total;
                    }
                }

                #endregion
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        private bool GetFieldsNew(byte[] data)
        {
            bool result = true;
            try
            {
                string TitleXML = Encoding.GetEncoding(1251).GetString(data);
                byte[] res = null;
                UniversalTransferDocumentWithHyphens res2 = null;
                try
                {
                    res = Connection.ParseTitleXml(Token, BoxID, en.DocumentInfo.TypeNamedId, en.DocumentInfo.Function, en.DocumentInfo.Version, 0, data);
                    string XML = Encoding.UTF8.GetString(res);
                    XML = XML.Replace("﻿<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", "");
                    XML = XML.Replace("UniversalTransferDocumentWithHyphens", "UniversalTransferDocument");
                    XML = XML.Replace("UniversalTransferDocument", "UniversalTransferDocumentWithHyphens");
                    XmlSerializer formatter = new XmlSerializer(typeof(UniversalTransferDocumentWithHyphens));
                    using (StringReader reader = new StringReader(XML))
                    {
                        res2 = formatter.Deserialize(reader) as UniversalTransferDocumentWithHyphens;
                    }
                }
                catch
                {
                    return false;
                }
                foreach (var row in res2.Table.Item)
                {
                    try
                    {
                        TableRow trow = new TableRow();
                        trow.Amount = row.Subtotal.ToString();
                        trow.AmountWOVAT = row.SubtotalWithVatExcluded.ToString();
                        trow.TaxRate = row.TaxRate.ToString();
                        trow.Quantity = row.Quantity.ToString();
                        trow.UnitName = row.UnitName;
                        trow.Name = row.Product;
                        trow.VAT = row.Vat.ToString();
                        trow.AmountToPay = row.Price.ToString();
                        rows.Add(trow);
                    }
                    catch (Exception)
                    {
                    }

                }
                summ = res2.Table.Total.ToString();
                VatAmount = res2.Table.Vat.ToString();
                SummWOVAT = res2.Table.TotalWithVatExcluded.ToString();
                Currency = res2.Currency;
                UniversalDocumentType = res2.Function.ToString();
                if (res2.TransferInfo != null && res2.TransferInfo.TransferBases != null && res2.TransferInfo.TransferBases.Count() > 0)
                {
                    var trbase = res2.TransferInfo.TransferBases[0];
                    Operation = new Operation(trbase);
                }
                ExtendedOrganizationDetails_ManualFilling Seller = null;
                if (res2.Sellers != null && res2.Sellers.Count() > 0)
                {
                    Seller = res2.Sellers[0].Item;
                    SellerAdress = ParseOrgAddress(Seller);
                    if (Seller is ExtendedOrganizationDetailsWithHyphens)
                    {
                        var o = Seller as ExtendedOrganizationDetailsWithHyphens;
                        Infopol.Add(new InfoRow("SellerINN", o.Inn));
                        Infopol.Add(new InfoRow("SellerKPP", o.Kpp));
                        Infopol.Add(new InfoRow("SellerName", o.OrgName));
                    }
                    else if (Seller is ExtendedOrganizationDetails)
                    {
                        var o = Seller as ExtendedOrganizationDetails;
                        Infopol.Add(new InfoRow("SellerINN", o.Inn));
                        Infopol.Add(new InfoRow("SellerKPP", o.Kpp));
                        Infopol.Add(new InfoRow("SellerName", o.OrgName));
                    }
                }

                if (res2.Shippers != null && res2.Shippers.Count() > 0)
                {
                    ExtendedOrganizationDetails_ManualFilling Shipper = null;
                    if (res2.Shippers[0].SameAsSeller == UniversalTransferDocumentWithHyphensShipperSameAsSeller.True)
                    {
                        Shipper = Seller;
                        ShipperAdress = SellerAdress;
                    }
                    else
                    {
                        Shipper = res2.Shippers[0].Item;
                        ShipperAdress = ParseOrgAddress(res2.Shippers[0].Item);
                    }
                    if (Shipper is ExtendedOrganizationDetailsWithHyphens)
                    {
                        var o = Shipper as ExtendedOrganizationDetailsWithHyphens;
                        Infopol.Add(new InfoRow("ShipperINN", o.Inn));
                        Infopol.Add(new InfoRow("ShipperKPP", o.Kpp));
                        Infopol.Add(new InfoRow("ShipperName", o.OrgName));
                    }
                    else if (Shipper is ExtendedOrganizationDetails)
                    {
                        var o = Shipper as ExtendedOrganizationDetails;
                        Infopol.Add(new InfoRow("ShipperINN", o.Inn));
                        Infopol.Add(new InfoRow("ShipperKPP", o.Kpp));
                        Infopol.Add(new InfoRow("ShipperName", o.OrgName));
                    }
                }
                if (res2.Buyers != null && res2.Buyers.Count() > 0)
                    BuyerAdress = ParseOrgAddress(res2.Buyers[0].Item);
                if (res2.Consignees != null && res2.Consignees.Count() > 0)
                    ConsigneeAdress = ParseOrgAddress(res2.Consignees[0].Item);

                if (res2.AdditionalInfoId != null && res2.AdditionalInfoId.AdditionalInfo != null)
                    foreach (var inf in res2.AdditionalInfoId.AdditionalInfo)
                    {
                        Infopol.Add(new InfoRow(inf.Id, inf.Value));
                    }
                if (res2.TransferInfo != null && res2.TransferInfo.AdditionalInfoId != null)
                    foreach (var inf in res2.TransferInfo.AdditionalInfoId.AdditionalInfo)
                    {
                        Infopol.Add(new InfoRow(inf.Id, inf.Value));
                    }

                //НомерТД
                if (res2.Table != null && res2.Table.Item != null)
                {
                    string Declarations = "";
                    foreach (var item in res2.Table.Item)
                    {
                        if (item.CustomsDeclarations != null)
                        {
                            foreach (var Declaration in item.CustomsDeclarations)
                            {
                                if (!string.IsNullOrEmpty(Declaration.DeclarationNumber))
                                {
                                    if (!string.IsNullOrEmpty(Declarations))
                                        Declarations += ";";
                                    Declarations += Declaration.DeclarationNumber;
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(Declarations))
                        Infopol.Add(new InfoRow("CustomsNumbers", Declarations));
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public Address ParseOrgAddress(ExtendedOrganizationDetails_ManualFilling org)
        {
            try
            {
                if (org is ExtendedOrganizationDetailsWithHyphens)
                {
                    var o = org as ExtendedOrganizationDetailsWithHyphens;
                    if (o.Address.Item is RussianAddress)
                    {
                        var adress = o.Address.Item as RussianAddress;
                        return new Address(adress);
                    }
                    else if (o.Address.Item is ForeignAddress)
                    {
                        var adress = o.Address.Item as ForeignAddress;
                        return new Address(adress);
                    }
                }
                else
                {
                    var o = org as ExtendedOrganizationDetails;
                    if (o.Address.Item is RussianAddress)
                    {
                        var adress = o.Address.Item as RussianAddress;
                        return new Address(adress);
                    }
                    else if (o.Address.Item is ForeignAddress)
                    {
                        var adress = o.Address.Item as ForeignAddress;
                        return new Address(adress);
                    }
                }
            }
            catch (Exception)
            {
            }
            return new Address();
        }

        public void ParseUCDOldFormat(UniversalCorrectionDocumentSellerTitleInfo UTD)
        {
            double TotalsIncTotal = string.IsNullOrEmpty(UTD.InvoiceCorrectionTable.TotalsInc.Total) ? 0 : double.Parse(UTD.InvoiceCorrectionTable.TotalsInc.Total, CultureInfo.InvariantCulture);
            double TotalsDecTotal = string.IsNullOrEmpty(UTD.InvoiceCorrectionTable.TotalsDec.Total) ? 0 : double.Parse(UTD.InvoiceCorrectionTable.TotalsDec.Total, CultureInfo.InvariantCulture);
            double TotalsIncVat = string.IsNullOrEmpty(UTD.InvoiceCorrectionTable.TotalsInc.Vat) ? 0 : double.Parse(UTD.InvoiceCorrectionTable.TotalsInc.Vat, CultureInfo.InvariantCulture);
            double TotalsDecVat = string.IsNullOrEmpty(UTD.InvoiceCorrectionTable.TotalsDec.Vat) ? 0 : double.Parse(UTD.InvoiceCorrectionTable.TotalsDec.Vat, CultureInfo.InvariantCulture);
            double TotalsIncTotalWithVatExcluded = string.IsNullOrEmpty(UTD.InvoiceCorrectionTable.TotalsInc.TotalWithVatExcluded) ? 0 : double.Parse(UTD.InvoiceCorrectionTable.TotalsInc.TotalWithVatExcluded, CultureInfo.InvariantCulture);
            double TotalsDecTotalWithVatExcluded = string.IsNullOrEmpty(UTD.InvoiceCorrectionTable.TotalsDec.TotalWithVatExcluded) ? 0 : double.Parse(UTD.InvoiceCorrectionTable.TotalsDec.TotalWithVatExcluded, CultureInfo.InvariantCulture);
            summ = (TotalsIncTotal - TotalsDecTotal).ToString();
            VatAmount = (TotalsIncVat - TotalsDecVat).ToString();
            Currency = UTD.Currency;
            SummWOVAT = (TotalsIncTotalWithVatExcluded - TotalsDecTotalWithVatExcluded).ToString();
            if (UTD.AdditionalInfoId != null)
                foreach (var inf in UTD.AdditionalInfoId.AdditionalInfo)
                {
                    Infopol.Add(new InfoRow(inf.Id, inf.Value));
                }
            if (UTD.Seller != null && UTD.Seller.Address != null && UTD.Seller.Address.RussianAddress != null)
                SellerAdress = new Address(UTD.Seller.Address.RussianAddress);
            if (UTD.Buyer != null && UTD.Buyer.Address != null && UTD.Buyer.Address.RussianAddress != null)
                BuyerAdress = new Address(UTD.Buyer.Address.RussianAddress);
            if (UTD.Seller != null && UTD.Seller.Address != null && UTD.Seller.Address.ForeignAddress != null)
                SellerAdress = new Address(UTD.Seller.Address.ForeignAddress);
            if (UTD.Buyer != null && UTD.Buyer.Address != null && UTD.Buyer.Address.ForeignAddress != null)
                BuyerAdress = new Address(UTD.Buyer.Address.ForeignAddress);
        }

        public void ParseUTDOldFormat(UniversalTransferDocumentSellerTitleInfo UTD)
        {
            summ = UTD.InvoiceTable.Total;
            VatAmount = UTD.InvoiceTable.Vat;
            Currency = UTD.Currency;
            SummWOVAT = UTD.InvoiceTable.TotalWithVatExcluded;
            if (UTD.AdditionalInfoId != null)
                foreach (var inf in UTD.AdditionalInfoId.AdditionalInfo)
                {
                    Infopol.Add(new InfoRow(inf.Id, inf.Value));
                }
            if (UTD.TransferInfo != null && UTD.TransferInfo.AdditionalInfoId != null)
                foreach (var inf in UTD.TransferInfo.AdditionalInfoId.AdditionalInfo)
                {
                    Infopol.Add(new InfoRow(inf.Id, inf.Value));
                }
            if (UTD.Seller != null && UTD.Seller.Address != null && UTD.Seller.Address.RussianAddress != null)
                SellerAdress = new Address(UTD.Seller.Address.RussianAddress);
            if (UTD.Consignee != null && UTD.Consignee.Address != null && UTD.Consignee.Address.RussianAddress != null)
                ConsigneeAdress = new Address(UTD.Consignee.Address.RussianAddress);
            if (UTD.Buyer != null && UTD.Buyer.Address != null && UTD.Buyer.Address.RussianAddress != null)
                BuyerAdress = new Address(UTD.Buyer.Address.RussianAddress);
            if (UTD.Shipper != null && UTD.Shipper.OrgInfo != null && UTD.Shipper.OrgInfo.Address != null && UTD.Shipper.OrgInfo.Address.RussianAddress != null)
                ShipperAdress = new Address(UTD.Shipper.OrgInfo.Address.RussianAddress);
            if (UTD.Seller != null && UTD.Seller.Address != null && UTD.Seller.Address.ForeignAddress != null)
                SellerAdress = new Address(UTD.Seller.Address.ForeignAddress);
            if (UTD.Consignee != null && UTD.Consignee.Address != null && UTD.Consignee.Address.ForeignAddress != null)
                ConsigneeAdress = new Address(UTD.Consignee.Address.ForeignAddress);
            if (UTD.Buyer != null && UTD.Buyer.Address != null && UTD.Buyer.Address.ForeignAddress != null)
                BuyerAdress = new Address(UTD.Buyer.Address.ForeignAddress);
            if (UTD.Shipper != null && UTD.Shipper.OrgInfo != null && UTD.Shipper.OrgInfo.Address != null && UTD.Shipper.OrgInfo.Address.ForeignAddress != null)
                ShipperAdress = new Address(UTD.Shipper.OrgInfo.Address.ForeignAddress);
        }

        public void ParseInvoiceInfo(InvoiceInfo info)
        {
            summ = info.Total;
            VatAmount = info.Vat;
            Currency = info.Currency;
            UniversalDocumentType = "";
            if (info.AdditionalInfos != null)
                foreach (var inf in info.AdditionalInfos)
                {
                    Infopol.Add(new InfoRow(inf.Id, inf.Value));
                }
            if (info.Seller != null && info.Seller.OrgInfo != null && info.Seller.OrgInfo.Address != null && info.Seller.OrgInfo.Address.RussianAddress != null)
                SellerAdress = new Address(info.Seller.OrgInfo.Address.RussianAddress);
            if (info.Consignee != null && info.Consignee.OrgInfo != null && info.Consignee.OrgInfo.Address != null && info.Consignee.OrgInfo.Address.RussianAddress != null)
                ConsigneeAdress = new Address(info.Consignee.OrgInfo.Address.RussianAddress);
            if (info.Buyer != null && info.Consignee.OrgInfo != null && info.Buyer.OrgInfo.Address != null && info.Buyer.OrgInfo.Address.RussianAddress != null)
                BuyerAdress = new Address(info.Buyer.OrgInfo.Address.RussianAddress);
            if (info.Shipper != null && info.Shipper.OrgInfo != null && info.Shipper.OrgInfo.Address != null && info.Shipper.OrgInfo.Address.RussianAddress != null)
                ShipperAdress = new Address(info.Shipper.OrgInfo.Address.RussianAddress);
            if (info.Seller != null && info.Seller.OrgInfo != null && info.Seller.OrgInfo.Address != null && info.Seller.OrgInfo.Address.ForeignAddress != null)
                SellerAdress = new Address(info.Seller.OrgInfo.Address.ForeignAddress);
            if (info.Consignee != null && info.Consignee.OrgInfo != null && info.Consignee.OrgInfo.Address != null && info.Consignee.OrgInfo.Address.ForeignAddress != null)
                ConsigneeAdress = new Address(info.Consignee.OrgInfo.Address.ForeignAddress);
            if (info.Buyer != null && info.Consignee.OrgInfo != null && info.Buyer.OrgInfo.Address != null && info.Buyer.OrgInfo.Address.ForeignAddress != null)
                BuyerAdress = new Address(info.Buyer.OrgInfo.Address.ForeignAddress);
            if (info.Shipper != null && info.Shipper.OrgInfo != null && info.Shipper.OrgInfo.Address != null && info.Shipper.OrgInfo.Address.ForeignAddress != null)
                ShipperAdress = new Address(info.Shipper.OrgInfo.Address.ForeignAddress);
        }

        public void ParseAcceptanceCertificate552Format(AcceptanceCertificate552SellerTitleInfo info)
        {
            double summd = 0;
            double vat = 0;
            foreach (AcceptanceCertificate552WorkDescription item in info.Works)
            {
                summd += string.IsNullOrEmpty(item.Total) ? 0 : double.Parse(item.Total.Replace(",", "."), CultureInfo.InvariantCulture);
                vat += string.IsNullOrEmpty(item.TotalVat) ? 0 : double.Parse(item.TotalVat.Replace(",", "."), CultureInfo.InvariantCulture);
            }
            summ = summd.ToString();
            VatAmount = vat.ToString();
            SummWOVAT = (summd - vat).ToString();
            Currency = info.Currency;
            UniversalDocumentType = "";
            if (info.AdditionalInfoId != null)
                foreach (var inf in info.AdditionalInfoId.AdditionalInfo)
                {
                    Infopol.Add(new InfoRow(inf.Id, inf.Value));
                }
            if (info.TransferInfo != null && info.TransferInfo.AdditionalInfos != null)
                foreach (var inf in info.TransferInfo.AdditionalInfos)
                {
                    Infopol.Add(new InfoRow(inf.Id, inf.Value));
                }
            if (info.Seller != null && info.Seller.Address != null && info.Seller.Address.RussianAddress != null)
                SellerAdress = new Address(info.Seller.Address.RussianAddress);
            if (info.Buyer != null && info.Buyer.Address != null && info.Buyer.Address.RussianAddress != null)
                BuyerAdress = new Address(info.Buyer.Address.RussianAddress);
            if (info.Seller != null && info.Seller.Address != null && info.Seller.Address.ForeignAddress != null)
                SellerAdress = new Address(info.Seller.Address.ForeignAddress);
            if (info.Buyer != null && info.Buyer.Address != null && info.Buyer.Address.ForeignAddress != null)
                BuyerAdress = new Address(info.Buyer.Address.ForeignAddress);
        }

        public void ParseAcceptanceCertificateFormat(AcceptanceCertificateSellerTitleInfo info)
        {
            double summd = 0;
            double vat = 0;
            foreach (WorkDescription item in info.Works)
            {
                summ += string.IsNullOrEmpty(item.Total) ? 0 : double.Parse(item.Total.Replace(",", "."), CultureInfo.InvariantCulture);
                vat += string.IsNullOrEmpty(item.Vat) ? 0 : double.Parse(item.Vat.Replace(",", "."), CultureInfo.InvariantCulture);
            }
            summ = summd.ToString();
            VatAmount = vat.ToString();
            UniversalDocumentType = "";
            Infopol.Add(new InfoRow("_", info.AdditionalInfo));
            if (info.Seller != null && info.Seller.OrgInfo != null && info.Seller.OrgInfo.Address != null && info.Seller.OrgInfo.Address.RussianAddress != null)
                SellerAdress = new Address(info.Seller.OrgInfo.Address.RussianAddress);
            if (info.Seller != null && info.Seller.OrgInfo != null && info.Seller.OrgInfo.Address != null && info.Seller.OrgInfo.Address.ForeignAddress != null)
                SellerAdress = new Address(info.Seller.OrgInfo.Address.ForeignAddress);
        }

        public void ParseTovTorgSellerTitleInfoFormat(TovTorgSellerTitleInfo info)
        {
            summ = info.Table.Total;
            VatAmount = info.Table.TotalVat;
            Currency = info.Currency;
            UniversalDocumentType = "";
            if (info.AdditionalInfoId != null)
                foreach (var inf in info.AdditionalInfoId.AdditionalInfo)
                {
                    Infopol.Add(new InfoRow(inf.Id, inf.Value));
                }
            if (info.TransferInfo != null && info.TransferInfo.AdditionalInfos != null)
                foreach (var inf in info.TransferInfo.AdditionalInfos)
                {
                    Infopol.Add(new InfoRow(inf.Id, inf.Value));
                }
            if (info.Seller != null && info.Seller.Address != null && info.Seller.Address.RussianAddress != null)
                SellerAdress = new Address(info.Seller.Address.RussianAddress);
            if (info.Consignee != null && info.Consignee.Address != null && info.Consignee.Address.RussianAddress != null)
                ConsigneeAdress = new Address(info.Consignee.Address.RussianAddress);
            if (info.Buyer != null && info.Buyer.Address != null && info.Buyer.Address.RussianAddress != null)
                BuyerAdress = new Address(info.Buyer.Address.RussianAddress);
            if (info.Shipper != null && info.Shipper.Address != null && info.Shipper.Address.RussianAddress != null)
                ShipperAdress = new Address(info.Shipper.Address.RussianAddress);
            if (info.Seller != null && info.Seller.Address != null && info.Seller.Address.ForeignAddress != null)
                SellerAdress = new Address(info.Seller.Address.ForeignAddress);
            if (info.Consignee != null && info.Consignee.Address != null && info.Consignee.Address.ForeignAddress != null)
                ConsigneeAdress = new Address(info.Consignee.Address.ForeignAddress);
            if (info.Buyer != null && info.Buyer.Address != null && info.Buyer.Address.ForeignAddress != null)
                BuyerAdress = new Address(info.Buyer.Address.ForeignAddress);
            if (info.Shipper != null && info.Shipper.Address != null && info.Shipper.Address.ForeignAddress != null)
                ShipperAdress = new Address(info.Shipper.Address.ForeignAddress);
        }

        public void ParseTorg12SellerTitleInfoFormat(Torg12SellerTitleInfo info)
        {
            summ = info.Total;
            VatAmount = info.Vat;
            UniversalDocumentType = "";
            Infopol.Add(new InfoRow("_", info.AdditionalInfo));
            if (info.Supplier != null && info.Supplier.Address != null && info.Supplier.Address.RussianAddress != null)
                SellerAdress = new Address(info.Supplier.Address.RussianAddress);
            if (info.Consignee != null && info.Consignee.Address != null && info.Consignee.Address.RussianAddress != null)
                ConsigneeAdress = new Address(info.Consignee.Address.RussianAddress);
            if (info.Payer != null && info.Payer.Address != null && info.Payer.Address.RussianAddress != null)
                BuyerAdress = new Address(info.Payer.Address.RussianAddress);
            if (info.Shipper != null && info.Shipper.Address != null && info.Shipper.Address.RussianAddress != null)
                ShipperAdress = new Address(info.Shipper.Address.RussianAddress);
            if (info.Supplier != null && info.Supplier.Address != null && info.Supplier.Address.ForeignAddress != null)
                SellerAdress = new Address(info.Supplier.Address.ForeignAddress);
            if (info.Consignee != null && info.Consignee.Address != null && info.Consignee.Address.ForeignAddress != null)
                ConsigneeAdress = new Address(info.Consignee.Address.ForeignAddress);
            if (info.Payer != null && info.Payer.Address != null && info.Payer.Address.ForeignAddress != null)
                BuyerAdress = new Address(info.Payer.Address.ForeignAddress);
            if (info.Shipper != null && info.Shipper.Address != null && info.Shipper.Address.ForeignAddress != null)
                ShipperAdress = new Address(info.Shipper.Address.ForeignAddress);
        }

        /// <summary>
        /// Метод получения метаданных по имени.
        /// </summary>
        /// <param name="Key">Имя Метаданных</param>
        /// <returns>Значение метаданных</returns>
        public string GetMetadata(string Key)
        {
            string retVal = "";
            try
            {
                foreach (InfoRow row in Infopol)
                {
                    if (row.Key.ToLower().Trim() == Key.ToLower().Trim())
                        retVal = row.Value;
                }
            }
            catch { }
            return retVal;
        }

        /// <summary>
        /// Загружает данные для текущего объекта из ХМЛ
        /// </summary>
        /// <param name="XML">Хмлка с данными</param>
        private void DeserializeFromXML(string XML)
        {
            XMLMetadata data = new XMLMetadata();
            XmlSerializer ser = new XmlSerializer(data.GetType());
            using (StringReader reader = new StringReader(XML))
            {
                data = ser.Deserialize(reader) as XMLMetadata;
            }
            Infopol = data.Infopol;
            ConsigneeAdress = data.ConsigneeAdress;
            SellerAdress = data.SellerAdress;
            ConsigneeAdress = data.ConsigneeAdress;
            BuyerAdress = data.BuyerAdress;
        }
    }

    [Serializable]
    public class Address
    {
        [XmlAttribute]
        public string Apartment { get; set; }
        [XmlAttribute]
        public string Block { get; set; }
        [XmlAttribute]
        public string Building { get; set; }
        [XmlAttribute]
        public string City { get; set; }
        [XmlAttribute]
        public string Locality { get; set; }
        [XmlAttribute]
        public string Region { get; set; }
        [XmlAttribute]
        public string Street { get; set; }
        [XmlAttribute]
        public string Territory { get; set; }
        [XmlAttribute]
        public string ZipCode { get; set; }
        [XmlAttribute]
        public string Country { get; set; }
        [XmlAttribute]
        public string AdressString { get; set; }

        public Address()
        {

        }

        public Address(RussianAddress russianAddress)
        {
            Apartment = russianAddress.Apartment;
            Block = russianAddress.Block;
            Building = russianAddress.Building;
            City = russianAddress.City;
            Locality = russianAddress.Locality;
            Region = russianAddress.Region;
            Street = russianAddress.Street;
            Territory = russianAddress.Territory;
            ZipCode = russianAddress.ZipCode;
            SetAdressString();
        }

        public Address(ForeignAddress foreignAddress)
        {
            AdressString = foreignAddress.Address;
            Country = foreignAddress.Country;
        }

        public Address(Diadoc.Api.Proto.RussianAddress russianAddress)
        {
            Apartment = russianAddress.Apartment;
            Block = russianAddress.Block;
            Building = russianAddress.Building;
            City = russianAddress.City;
            Locality = russianAddress.Locality;
            Region = russianAddress.Region;
            Street = russianAddress.Street;
            Territory = russianAddress.Territory;
            ZipCode = russianAddress.ZipCode;
            SetAdressString();
        }

        public Address(Diadoc.Api.Proto.ForeignAddress foreignAddress)
        {
            AdressString = foreignAddress.Address;
            Country = foreignAddress.Country;
        }

        public void SetAdressString()
        {
            if (string.IsNullOrEmpty(ZipCode) && string.IsNullOrEmpty(Region) && string.IsNullOrEmpty(Territory) &&
                string.IsNullOrEmpty(City) && string.IsNullOrEmpty(Locality) && string.IsNullOrEmpty(Street) &&
                string.IsNullOrEmpty(Apartment) && string.IsNullOrEmpty(Block) && string.IsNullOrEmpty(Building))
                AdressString = "";
            else
                AdressString = "Индекс: " + ZipCode + ", Регион: " + Region + ", Район: " + Territory + ", Город: " + City + ", Населенный пункт: " + Locality + ", Улица: " + Street + ", Дом: " + Building + ", Корпус: " + Block + ", Офис: " + Apartment;
        }

        public override string ToString()
        {
            return AdressString;
        }
    }

    [Serializable]
    public class Operation
    {
        [XmlAttribute]
        public string BaseDocumentName { get; set; }
        [XmlAttribute]
        public string BaseDocumentDate { get; set; }
        [XmlAttribute]
        public string BaseDocumentId { get; set; }
        [XmlAttribute]
        public string BaseDocumentInfo { get; set; }
        [XmlAttribute]
        public string BaseDocumentNumber { get; set; }

        public Operation()
        {

        }
        public Operation(TransferBase820 trbase)
        {
            BaseDocumentDate = trbase.BaseDocumentDate;
            BaseDocumentId = trbase.BaseDocumentId;
            BaseDocumentInfo = trbase.BaseDocumentInfo;
            BaseDocumentName = trbase.BaseDocumentName;
            BaseDocumentNumber = trbase.BaseDocumentNumber;
        }

        public override string ToString()
        {
            return "НаимОсн=" + BaseDocumentName + ", НомОсн= " + BaseDocumentNumber + ", ДатаОсн=" + BaseDocumentDate + ", ДопСвОсн=" + BaseDocumentInfo;
        }
    }
    [Serializable]
    public class InfoRow
    {
        [XmlAttribute]
        public string Key { get; set; }
        [XmlText]
        public string Value { get; set; }

        public InfoRow()
        {

        }
        public InfoRow(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
