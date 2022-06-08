using System;
using System.Linq;
using DATABASE.Context;
using CloudArchive.Services;
using ARCHIVE.COMMON.Entities;
using System.Collections.Generic;
using ARCHIVE.COMMON.DTOModels.UI;
using System.IO;
using System.Xml;
using System.Collections;
using ARCHIVE.COMMON.DTOModels.Admin;
using Newtonsoft.Json;
using CloudArchive.Services.EDI.EnsolDiadoc;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace BackgroudServices.ScheduledTasks.OCR.Common
{
    public static partial class OCRCommon
    {
        public static void ParseXMLFile(NonFormDocs NonFormDoc, IBackgroundServiceLog _backgroundServiceLog, SearchServiceDBContext _dbContext, string ServiceName)
        {
            List<OCRMetadataDTO> documents = new List<OCRMetadataDTO>();
            using (TextReader sr = new StringReader(NonFormDoc.OCRXML))
            {
                try
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(sr);
                    var array = xdoc.ChildNodes[1];

                    XMLDataDescription data = ReadXML(array, NonFormDoc, _backgroundServiceLog, _dbContext, ServiceName);
                    NonFormDoc.OCRState = "Распознано";
                    if (data.Organization == null)
                    {
                        NonFormDoc.OCRState = "Ошибка определения типа документа";
                    }
                    if (data.Documents.Count == 0)
                    {
                        throw new Exception("Ни найдено ни 1 подходящего документа");
                    }
                    foreach (XMLDocumentDescription doc in data.Documents)
                    {

                        OCRMetadataDTO newdoc = new OCRMetadataDTO();
                        newdoc.ClientId = NonFormDoc.ClientId.Value;
                        newdoc.TablePart = doc.TablePart;
                        newdoc.Organization = data.Organization;
                        newdoc.Contractor = data.Contractor;
                        newdoc.OCRtype = doc.DocClass;
                        newdoc.OrganizationINN = doc.OrganizationINN;
                        newdoc.OrganizationKPP = doc.OrganizationKPP;
                        newdoc.OrganizationName = doc.OrganizationName;
                        newdoc.ContractorName = doc.ContractorName;
                        newdoc.ContractorINN = doc.ContractorINN;
                        newdoc.ContractorKPP = doc.ContractorKPP;
                        newdoc.AmountTotal = doc.NamedData["amount_total"] == null ? "" : doc.NamedData["amount_total"].ToString();
                        if (data.isIncoming.HasValue)
                        {
                            if (data.isIncoming.Value)
                            {
                                if (doc.DocClass == "Счет")
                                {
                                    newdoc.DocTypeId = 14;
                                    newdoc.DocType = "Счет входящий";
                                }
                                else if (doc.DocClass == "Счет-фактура")
                                {
                                    newdoc.DocTypeId = 15;
                                    newdoc.DocType = "Счет-фактура входящий";
                                }
                                else if (doc.DocClass == "ТТН")
                                {
                                    newdoc.DocTypeId = 20;
                                    newdoc.DocKind = new DocKind() { Id = 55 };
                                    newdoc.DocType = "Складской документ";
                                }
                                else
                                {
                                    newdoc.DocTypeId = 1;
                                    newdoc.DocType = "Поступление";
                                }
                            }
                            else
                            {
                                if (doc.DocClass == "Счет")
                                {
                                    newdoc.DocTypeId = 6;
                                    newdoc.DocType = "Счет исходящий";
                                }
                                else if (doc.DocClass == "Счет-фактура")
                                {
                                    newdoc.DocTypeId = 5;
                                    newdoc.DocType = "Счет-фактура исходящий";
                                }
                                else if (doc.DocClass == "ТТН")
                                {
                                    newdoc.DocTypeId = 20;
                                    newdoc.DocKind = new DocKind() { Id = 55 };
                                    newdoc.DocType = "Складской документ";
                                }
                                else
                                {
                                    newdoc.DocTypeId = 2;
                                    newdoc.DocType = "Реализация";
                                }
                            }
                        }

                        //currency
                        string currencyuncat = doc.NamedData["currency"] == null ? "" : doc.NamedData["currency"].ToString();
                        string currencynumber = currencyuncat;
                        if (currencyuncat.Contains(","))
                            currencynumber = currencyuncat.Substring(currencyuncat.LastIndexOf(',') + 1).Trim();
                        newdoc.Currency = DiadocMapping.GetCurrencyShortName(currencynumber);
                        newdoc.Currency = string.IsNullOrEmpty(newdoc.Currency) ? "RUB" : newdoc.Currency;

                        //parse mapped data
                        foreach (var fielddata in OCRCommon.MappingFields)
                        {
                            if (doc.NamedData[fielddata.Key] != null && doc.NamedData[fielddata.Key].ToString() != "")
                            {
                                var value = doc.NamedData[fielddata.Key].ToString();
                                ParseField(fielddata, value, newdoc);
                            }
                        }

                        foreach (var fielddata in OCRCommon.MappingInvoiceFields)
                        {
                            if (data.InvoiceData[fielddata.Key] != null && data.InvoiceData[fielddata.Key].ToString() != "")
                            {
                                var value = data.InvoiceData[fielddata.Key].ToString();
                                ParseField(fielddata, value, newdoc);
                            }
                        }
                        foreach (var fielddata in OCRCommon.MappingTaxInvoiceFields)
                        {
                            if (data.TaxInvoiceData[fielddata.Key] != null && data.TaxInvoiceData[fielddata.Key].ToString() != "")
                            {
                                var value = data.TaxInvoiceData[fielddata.Key].ToString();
                                ParseField(fielddata, value, newdoc);
                            }
                        }
                        documents.Add(newdoc);
                    }
                    if (documents[0].Organization != null)
                        NonFormDoc.OrganizationId = documents[0].Organization.Id;
                    if (documents[0].Contractor != null)
                        NonFormDoc.ContractorId = documents[0].Contractor.Id;
                    if (documents[0].Amount != null)
                        NonFormDoc.Amount = documents[0].Amount;
                    if (documents[0].DocDate != null)
                        NonFormDoc.DocDate = documents[0].DocDate;
                    if (documents[0].DocTypeId > 0)
                        NonFormDoc.DocTypeId = documents[0].DocTypeId;
                    NonFormDoc.OCRVerified = JsonConvert.SerializeObject(documents, Newtonsoft.Json.Formatting.None);
                }
                catch (Exception e)
                {
                    NonFormDoc.OCRState = "Ошибка обработки результатов";
                    _backgroundServiceLog.AddError("Произошла ошибка во время обработки результатов: " + e.Message + " StackTrace: " + e.StackTrace, ServiceName);
                }
            }
        }

        private static XMLDataDescription ReadXML(XmlNode array, NonFormDocs NonFormDoc, IBackgroundServiceLog _backgroundServiceLog, SearchServiceDBContext _dbContext, string ServiceName)
        {
            XMLDataDescription data = new XMLDataDescription();
            int BestWeight = 0;
            foreach (XmlNode node in array.ChildNodes)
            {
                XMLDocumentDescription doc = new XMLDocumentDescription();
                XmlNodeList NamedNodes = null;
                try
                {
                    foreach (XmlNode cn in node.ChildNodes)
                    {
                        try
                        {
                            if (cn.Name == "Data")
                            {
                                NamedNodes = cn.ChildNodes;
                            }
                            else if (cn.Name == "TablesData")
                            {
                                if (cn.ChildNodes.Count > 0)
                                    doc.TablePart = ParseTablePart(cn.ChildNodes[0].ChildNodes[1].ChildNodes);
                            }
                        }
                        catch (Exception ex)
                        {
                            _backgroundServiceLog.AddError("Ошибка в структуре XML!Тег: " + cn.Name + ". Error:" + ex.Message + " StackTrace: " + ex.StackTrace, ServiceName);
                            throw ex;
                        }
                    }
                    foreach (XmlNode cn in NamedNodes)
                    {
                        var Title = cn.FirstChild.InnerText;
                        var value = "";
                        foreach (XmlNode val in cn.LastChild.ChildNodes)
                        {
                            if (val.Name == "Text")
                                value = val.InnerText;
                        }
                        doc.NamedData[Title] = value;
                    }

                    doc.DocClass = doc.NamedData["doc_type"].ToString();

                    if (doc.DocClass == "Счет-фактура")
                    {
                        data.TaxInvoiceData = doc.NamedData;
                    }
                    if (doc.DocClass == "Счет")
                    {
                        data.InvoiceData = doc.NamedData;
                    }

                    doc.OUT_INN = doc.NamedData["out_inn"] == null ? "" : doc.NamedData["out_inn"].ToString();
                    doc.OUT_KPP = doc.NamedData["out_kpp"] == null ? "" : doc.NamedData["out_kpp"].ToString();
                    doc.OUT_Name = doc.NamedData["out_name"] == null ? "" : doc.NamedData["out_name"].ToString();

                    doc.IN_INN = doc.NamedData["in_inn"] == null ? "" : doc.NamedData["in_inn"].ToString();
                    doc.IN_KPP = doc.NamedData["in_kpp"] == null ? "" : doc.NamedData["in_kpp"].ToString();
                    doc.IN_Name = doc.NamedData["in_name"] == null ? "" : doc.NamedData["in_name"].ToString();

                    if (GetWeight(doc.DocClass, ref BestWeight))
                    {
                        data.Documents.Insert(0, doc);
                    }
                    else
                    {
                        data.Documents.Add(doc);
                    }
                }
                catch (Exception ex)
                {
                    _backgroundServiceLog.AddError("Произошла ошибка во время работы парсинга XML! Error: " + ex.Message + " StackTrace: " + ex.StackTrace, ServiceName);
                    throw ex;
                }
            }
            foreach (XMLDocumentDescription doc in data.Documents)
            {
                try
                {
                    var organization = new OrganizationDTO();
                    Func<Organization, bool> orgkpp;
                    Func<Organization, bool> orginn;
                    Func<Organization, bool> innkpp;
                    doc.OrganizationINN = "";
                    doc.OrganizationKPP = "";
                    if (!string.IsNullOrEmpty(doc.IN_INN))
                    {
                        orginn = x => ((string)x.INN) == doc.IN_INN;
                        if (!string.IsNullOrEmpty(doc.IN_KPP))
                        {
                            orgkpp = x => ((string)x.KPP) == doc.IN_KPP;
                            innkpp = x => orginn(x) && orgkpp(x) && x.ClientId == NonFormDoc.ClientId;
                        }
                        else
                        {
                            innkpp = x => orginn(x) && x.ClientId == NonFormDoc.ClientId;
                        }
                        var org = _dbContext.Organizations.AsNoTracking().FirstOrDefault(innkpp);
                        if (org != null)
                        {
                            organization.Name = org.Name;
                            organization.Id = org.Id;
                            organization.INN = doc.IN_INN;
                            organization.KPP = doc.IN_KPP;
                            data.Organization = organization;
                            data.isIncoming = true;
                            doc.OrganizationINN = doc.IN_INN;
                            doc.OrganizationKPP = doc.IN_KPP;
                            doc.OrganizationName = doc.IN_Name;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(doc.OUT_INN))
                    {
                        orginn = x => ((string)x.INN) == doc.OUT_INN;
                        if (!string.IsNullOrEmpty(doc.OUT_KPP))
                        {
                            orgkpp = x => ((string)x.KPP) == doc.OUT_KPP;
                            innkpp = x => orginn(x) && orgkpp(x) && x.ClientId == NonFormDoc.ClientId;
                        }
                        else
                            innkpp = x => orginn(x) && x.ClientId == NonFormDoc.ClientId;
                        var org = _dbContext.Organizations.AsNoTracking().FirstOrDefault(innkpp);
                        if (org != null)
                        {
                            organization.Name = org.Name;
                            organization.Id = org.Id;
                            organization.INN = doc.OUT_INN;
                            organization.KPP = doc.OUT_KPP;
                            doc.OrganizationINN = doc.OUT_INN;
                            doc.OrganizationKPP = doc.OUT_KPP;
                            doc.OrganizationName = doc.OUT_Name;
                            data.Organization = organization;
                            data.isIncoming = false;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _backgroundServiceLog.AddError("Произошла ошибка во время поиска организации! Error: " + ex.Message + " StackTrace: " + ex.StackTrace, ServiceName);
                    throw ex;
                }
            }
            if (data.Organization != null)
            {
                foreach (XMLDocumentDescription doc in data.Documents)
                {
                    doc.ContractorINN = "";
                    doc.ContractorKPP = "";
                    try
                    {
                        var contractor = new ContractorDTO();
                        if (data.isIncoming.HasValue)
                        {
                            if (!data.isIncoming.Value)
                            {
                                contractor.INN = doc.IN_INN;
                                contractor.KPP = doc.IN_KPP;
                                contractor.Name = doc.IN_Name;
                                doc.ContractorName = doc.IN_Name;
                            }
                            else
                            {
                                contractor.INN = doc.OUT_INN;
                                contractor.KPP = doc.OUT_KPP;
                                contractor.Name = doc.OUT_Name;
                                doc.ContractorName = doc.OUT_Name;
                            }
                        }
                        if (!string.IsNullOrEmpty(contractor.INN))
                        {
                            Func<Contractor, bool> kpp;
                            Func<Contractor, bool> innkpp;
                            Func<Contractor, bool> inn = x => ((string)x.INN) == contractor.INN;
                            doc.ContractorINN = contractor.INN;
                            if (!string.IsNullOrEmpty(contractor.KPP))
                            {
                                doc.ContractorKPP = contractor.KPP;
                                kpp = x => ((string)x.KPP) == contractor.KPP;
                                innkpp = x => inn(x) && kpp(x) && x.OrganizationId == data.Organization.Id;
                            }
                            else
                                innkpp = x => inn(x) && x.OrganizationId == data.Organization.Id;
                            var ctg = _dbContext.Contractors.AsNoTracking().FirstOrDefault(innkpp);
                            if (ctg != null)
                            {
                                contractor.Name = ctg.Name;
                                contractor.Id = ctg.Id;
                                data.Contractor = contractor;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _backgroundServiceLog.AddError("Произошла ошибка во время поиска контрагента! Error: " + ex.Message + " StackTrace: " + ex.StackTrace, ServiceName);
                        throw ex;
                    }
                }
            }
            return data;
        }

        private static void ParseField(KeyValuePair<string, Fieldinfo> data, string value, OCRMetadataDTO doc)
        {
            if (data.Value.FieldType == FieldTypeEnum.Datetime)
            {
                DateTime dateValue = DateTime.ParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                typeof(OCRMetadataDTO).GetProperty(data.Value.Name).SetValue(doc, dateValue);
            }
            else if (data.Value.FieldType == FieldTypeEnum.Double)
            {
                double doubleValue = double.Parse(value);
                typeof(OCRMetadataDTO).GetProperty(data.Value.Name).SetValue(doc, doubleValue);
            }
            else
            {
                typeof(OCRMetadataDTO).GetProperty(data.Value.Name).SetValue(doc, value);
            }
        }

        private static bool GetWeight(string DocClass, ref int BestWeight)
        {
            int currentweight = 0;
            switch (DocClass)
            {
                case "УПД":
                    currentweight = 50;
                    break;
                case "ТН":
                    currentweight = 40;
                    break;
                case "Акт":
                    currentweight = 30;
                    break;
                case "Счет-фактура":
                    currentweight = 20;
                    break;
                case "Счет":
                    currentweight = 10;
                    break;
                case "ТТН":
                    currentweight = 5;
                    break;
                default:
                    currentweight = 0;
                    break;
            }
            if (currentweight > BestWeight)
                BestWeight = currentweight;
            return currentweight >= BestWeight;
        }

        private static string ParseTablePart(XmlNodeList TableNodes)
        {
            string Result = "";
            List<TableRow> rows = new List<TableRow>();
            TableRow currentrow = null;
            int index = 0;
            foreach (XmlNode cn in TableNodes)
            {
                var Title = cn.FirstChild.InnerText;
                var value = "";
                var currentindex = int.Parse(Title.Substring(Title.LastIndexOf("_") + 1));

                if (currentrow == null)
                {
                    currentrow = new TableRow();
                }

                if (currentindex > index)
                {
                    index = currentindex;
                    rows.Add(currentrow);
                    currentrow = new TableRow();
                }

                foreach (XmlNode val in cn.LastChild.ChildNodes)
                {
                    if (val.Name == "Text")
                        value = val.InnerText;
                }

                if (Title.Contains("pos_name"))
                {
                    currentrow.Name = value;
                }
                else if (Title.Contains("qty_otp"))
                {
                    currentrow.Quantity = value;
                }
                else if (Title.Contains("price"))
                {
                    currentrow.AmountToPay = value;
                }
                else if (Title.Contains("sum_wo_nds"))
                {
                    currentrow.AmountWOVAT = value;
                }
                else if (Title.Contains("tax_rate"))
                {
                    currentrow.TaxRate = value;
                }
                else if (Title.Contains("sum_nds"))
                {
                    currentrow.VAT = value;
                }
                else if (Title.Contains("sum_all"))
                {
                    currentrow.Amount = value;
                }
                else if (Title.Contains("code_okei"))
                {
                    currentrow.UnitName = value;
                }

            }
            if (currentrow != null)
                rows.Add(currentrow);
            if (rows.Count > 0)
                Result = JsonConvert.SerializeObject(rows, Newtonsoft.Json.Formatting.None);
            return Result;
        }
    }

    public class XMLDataDescription
    {
        public OrganizationDTO Organization = null;
        public ContractorDTO Contractor = null;
        public bool? isIncoming = null;
        public Hashtable InvoiceData = new Hashtable();
        public Hashtable TaxInvoiceData = new Hashtable();


        public List<XMLDocumentDescription> Documents = new List<XMLDocumentDescription>();
    }

    public class XMLDocumentDescription
    {
        public string DocClass = "";
        public string TablePart = "";

        public string IN_KPP = "";
        public string IN_INN = "";
        public string IN_Name = "";

        public string OUT_INN = "";
        public string OUT_KPP = "";
        public string OUT_Name = "";
        public string ContractorINN { get; set; }
        public string ContractorKPP { get; set; }
        public string ContractorName { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationINN { get; set; }
        public string OrganizationKPP { get; set; }

        public Hashtable NamedData = new Hashtable();
    }
}

