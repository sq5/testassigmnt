// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using CloudArchive.Services;
using COMMON.Common.Services.ContextService;
using COMMON.Common.Services.StorageService;
using COMMON.Models;
using DATABASE.Context;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BackgroudServices.ScheduledTasks.OCR.Common
{
    public static partial class OCRCommon
    {
        public static Dictionary<string, Fieldinfo> MappingFields
        {
            get
            {
                Dictionary<string, Fieldinfo> _MappingFields = new Dictionary<string, Fieldinfo>();
                _MappingFields.Add("doc_num", new Fieldinfo() { Name = "DocNumber", FieldType = FieldTypeEnum.String });
                _MappingFields.Add("doc_date", new Fieldinfo() { Name = "DocDate", FieldType = FieldTypeEnum.Datetime });
                _MappingFields.Add("sum_wo_nds", new Fieldinfo() { Name = "AmountWOVAT", FieldType = FieldTypeEnum.Double });
                _MappingFields.Add("sum_nds", new Fieldinfo() { Name = "VAT", FieldType = FieldTypeEnum.Double });
                _MappingFields.Add("sum_all", new Fieldinfo() { Name = "Amount", FieldType = FieldTypeEnum.Double });
                return _MappingFields;
            }
        }
        public static Dictionary<string, Fieldinfo> MappingInvoiceFields
        {
            get
            {
                Dictionary<string, Fieldinfo> _MappingFields = new Dictionary<string, Fieldinfo>();
                _MappingFields.Add("doc_date", new Fieldinfo() { Name = "DocDateInvoice", FieldType = FieldTypeEnum.Datetime });
                _MappingFields.Add("doc_num", new Fieldinfo() { Name = "DocNumInvoice", FieldType = FieldTypeEnum.String });
                _MappingFields.Add("sum_all", new Fieldinfo() { Name = "AmountToPay", FieldType = FieldTypeEnum.Double });
                return _MappingFields;
            }
        }
        public static string QuequeImport = "";
        public static string QuequeExport = "";
        public static Dictionary<string, Fieldinfo> MappingTaxInvoiceFields
        {
            get
            {
                Dictionary<string, Fieldinfo> _MappingFields = new Dictionary<string, Fieldinfo>();
                _MappingFields.Add("doc_date", new Fieldinfo() { Name = "DocDateTaxInvoice", FieldType = FieldTypeEnum.Datetime });
                _MappingFields.Add("doc_num", new Fieldinfo() { Name = "DocNumTaxInvoice", FieldType = FieldTypeEnum.String });
                return _MappingFields;
            }
        }

        public static IConnection CreateConnection(IConfiguration configuration, IBackgroundServiceLog _backgroundServiceLog, string ServiceName)
        {
            var HostName = "";
            var UserName = "";
            var Password = "";
            var Port = 0;
            try
            {
                HostName = configuration["OCRHostName"];
                UserName = configuration["OCRUserName"];
                Password = configuration["OCRPassword"];
                Port = int.Parse(configuration["OCRPort"]);
            }
            catch (Exception)
            {
                _backgroundServiceLog.AddError("Не заданы параметры  подключения", ServiceName);
            }
            try
            {
                var factory = new ConnectionFactory() { HostName = HostName, UserName = UserName, Password = Password, Port = Port };
                var connection = factory.CreateConnection();
                return connection;
            }
            catch (Exception ex)
            {
                _backgroundServiceLog.AddError("Ошибка подключения к серверу распознавания: " + ex.Message, ServiceName);
            }
            return null;
        }
        public static IModel ConnectToChannel(IConnection connection, IBackgroundServiceLog _backgroundServiceLog, string ServiceName)
        {
            try
            {
                var channel = connection.CreateModel();
                channel.QueueDeclare(queue: QuequeImport,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.QueueDeclare(queue: QuequeExport,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                return channel;
            }
            catch (Exception ex)
            {
                _backgroundServiceLog.AddError("Ошибка подключения к очереди: " + ex.Message, ServiceName);
            }
            return null;
        }

        public static void SendTORabbitMq(NonFormDocs doc, IModel channel, SearchServiceDBContext db, IStorageService<StoredFile> fileStorage, IBackgroundServiceLog _backgroundServiceLog)
        {
            try
            {
                var files = db.Files.Where(x => x.NonFormDocId == doc.Id);
                if (files.Any())
                {
                    var model = new ESDataModel();
                    var id = Guid.NewGuid().ToString();
                    var batch = new Batch();
                    model.TaskID = doc.Id.ToString();
                    model.Batches.Add(batch);
                    batch.Id = doc.Id.ToString();
                    bool split = !doc.OCRSplit.HasValue ? false : doc.OCRSplit.Value;
                    batch.Split = split;
                    var dbfile = files.FirstOrDefault();
                    var file = new InputFile();
                    file.FileName = dbfile.FileName;
                    file.FileLocation = dbfile.FileName;
                    byte[] imageBytes = fileStorage.GetFileAsync(dbfile).GetAwaiter().GetResult();
                    file.FileBody = Convert.ToBase64String(imageBytes);
                    imageBytes = null;
                    batch.Files.Add(file);

                    var stringMessage = JsonConvert.SerializeObject(model, Formatting.None);

                    model = null;
                    file = null;

                    var bytes = Encoding.UTF8.GetBytes(stringMessage);
                    stringMessage = null;

                    var props = channel.CreateBasicProperties();
                    props.Headers = new Dictionary<string, object>();
                    props.Headers.Add("id", id);
                    channel.BasicPublish(exchange: "",
                                         routingKey: QuequeImport,
                                         basicProperties: props,
                                         body: bytes);
                    doc.OCRState = "На распознавании";
                }
                else
                {
                    doc.OCRState = "Не найден файл для распознавания";
                }
            }
            catch (Exception ex)
            {
                doc.OCRState = "Ошибка отправки на распознавание";
                _backgroundServiceLog.AddError("Error in SendTORabbitMq " + ex.Message + "StackTrace: " + ex.StackTrace, "OCRSenderService", doc.ClientId.HasValue ? doc.ClientId.Value : 0);
            }
        }

        public static NonFormDocs ParseRabbitMqMessage(ESDataModel _model, SearchServiceDBContext _db, IStorageService<StoredFile> _fileStorage, IBackgroundServiceLog _backgroundServiceLog, IAdminService _adminService)
        {
            NonFormDocs doc = null;
            try
            {
                foreach (var batch in _model.Batches)
                {
                    int DocID;
                    bool Split = batch.Split;
                    if (int.TryParse(batch.Id, out DocID))
                    {
                        var docs = _db.NonFormDocs.Where(x => x.Id == DocID);
                        if (docs.Any())
                        {
                            doc = docs.FirstOrDefault();                                    
                            var olddoc = doc;
                            if (Split && batch.Files.Count > 0)
                            //создаем новый nonform и удаляем старый
                            {
                                NonFormDocsDTO nonFormDocsDTO = new NonFormDocsDTO();
                                nonFormDocsDTO.Modified = DateTime.Now;
                                nonFormDocsDTO.Created = DateTime.Now;
                                nonFormDocsDTO.ClientId = doc.ClientId.Value;
                                nonFormDocsDTO.Sender = doc.Sender;
                                nonFormDocsDTO.OrganizationId = doc.OrganizationId;
                                nonFormDocsDTO.OCRState = _model.Status;
                                var idNewDoc = _adminService.CreateAsyncInt32<NonFormDocsDTO, NonFormDocs>(nonFormDocsDTO).GetAwaiter().GetResult();
                                if (idNewDoc == -1)
                                {
                                    throw new Exception("Ошибка создания nonformdoc ид " + batch.Id);
                                }
                                else
                                {

                                    if (!olddoc.Deleted.HasValue || !olddoc.Deleted.Value)
                                    {
                                        olddoc.Deleted = true;
                                        olddoc.DeletedBy = "ExternalSystem";
                                        olddoc.OCRState = "Распознано";
                                        olddoc.DeleteDate = DateTime.Now;

                                    }
                                    doc = _db.NonFormDocs.Where(x => x.Id == idNewDoc).FirstOrDefault();
                                }
                            }
                            else
                            {
                                doc.OCRState = _model.Status;
                            }
                            try
                            {
                                foreach (var rfile in batch.Files)
                                {

                                    if (rfile.FileBody?.Length > 0)
                                    {
                                        var bytes = Convert.FromBase64String(rfile.FileBody);
                                        var files = _db.Files.Where(x => x.NonFormDocId == doc.Id);
                                        if (files.Any())
                                        {
                                            var file = files.FirstOrDefault();
                                            file.FileName = rfile.FileName;
                                            file.FileBin = bytes;
                                            file.FileSize = bytes.Length;
                                            _fileStorage.CreateOrUpdateAsync(file).GetAwaiter().GetResult();
                                        }
                                        else if (Split)
                                        {
                                            var file = new DocFile();
                                            file.FileName = rfile.FileName;
                                            file.NonFormDocId = doc.Id;
                                            file.FileBin = bytes;
                                            file.FileSize = bytes.Length;
                                            _fileStorage.CreateOrUpdateAsync(file).GetAwaiter().GetResult();
                                        }
                                        else
                                        {
                                            throw new Exception("Не найден файл в базе! NonformDocID:" + doc.Id);
                                        }
                                    }
                                }
                                foreach (var rfile in batch.ResultFiles)
                                {
                                    if (rfile.FileBody?.Length > 0)
                                    {
                                        var bytes = Convert.FromBase64String(rfile.FileBody);
                                        doc.OCRXML = Encoding.UTF8.GetString(bytes);
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                doc.OCRState = "Ошибка получения результатов";
                                _backgroundServiceLog.AddError("Error in ReadFromRabbitMq " + ex.Message + "StackTrace: " + ex.StackTrace, "OCRConsumerService", doc.ClientId.HasValue ? doc.ClientId.Value : 0);
                            }
                        }
                        else
                        {
                            throw new Exception("Не найден документ с ид " + batch.Id);
                        }
                    }
                    else
                    {
                        throw new Exception("Идентификатор документа не является числом! " + batch.Id);
                    }
                }

            }
            catch (Exception ex)
            {
                _backgroundServiceLog.AddError("Error in ParseRabbitMqMessage " + ex.Message + "StackTrace: " + ex.StackTrace, "OCRConsumerService", 0);
            }
            return doc;
        }
        public static ESDataModel ReadFromRabbitMq(IModel channel, SearchServiceDBContext db, IStorageService<StoredFile> fileStorage, IBackgroundServiceLog _backgroundServiceLog)
        {
            var Res = channel.BasicGet(QuequeExport, true);

            if (Res == null)
                return null;
            var body = Res.Body.ToArray();
            var msg = Encoding.UTF8.GetString(body);
            body = null;
            ESDataModel model = null;
            try
            {
                using (var reader = new JsonTextReader(new StringReader(msg)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    model = serializer.Deserialize<ESDataModel>(reader);
                }
                msg = null;
            }
            catch (Exception ex)
            {
                model = new ESDataModel();
                _backgroundServiceLog.AddError("Error in ReadFromRabbitMq " + ex.Message + "StackTrace: " + ex.StackTrace, "OCRConsumerService", 0);
            }
            return model;
        }
    }

    public class ESDataModel
    {
        public ESDataModel()
        {
            Batches = new List<Batch>();
        }
        public string TaskID { get; set; }
        public string Status
        {
            get
            {
                switch (StatusCode)
                {
                    case 100:
                        return "Обработка результатов";
                    case 200:
                        return "Ошибка распознавания";
                    case 300:
                        return "Ошибка открытия файла";
                    default:
                        return "UnknownStatus";
                }
            }
        }
        public int StatusCode { get; set; }
        public List<Batch> Batches { get; set; }

    }


    public class InputFile
    {
        public string FileName { get; set; }
        public string FileLocation { get; set; }
        // Base64 string
        public string FileBody { get; set; }
    }

    public class Batch
    {
        public Batch()
        {
            Files = new List<InputFile>();
        }
        public string Id { get; set; }
        public bool Split { get; set; }
        public List<InputFile> Files { get; set; }
        public List<InputFile> ResultFiles { get; set; }
    }

    public class Fieldinfo
    {
        public string Name;
        public FieldTypeEnum FieldType;
    }

    public class TableRow
    {
        [JsonIgnore]
        private string _name = "";
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(_name) ? "" : _name;
            }
            set { _name = value; }
        }

        [JsonIgnore]
        private string _unitname = "";
        public string UnitName
        {
            get
            {
                return string.IsNullOrEmpty(_unitname) ? "" : _unitname;
            }
            set { _unitname = value; }
        }
        [JsonIgnore]
        private string _quantity = "";
        public string Quantity
        {
            get
            {
                return string.IsNullOrEmpty(_quantity) ? "" : _quantity;
            }
            set { _quantity = value; }
        }

        [JsonIgnore]
        private string _amountToPay = "0";
        public string AmountToPay
        {
            get
            {
                return string.IsNullOrEmpty(_amountToPay) ? "0" : _amountToPay.Replace(",", ".");
            }
            set { _amountToPay = value; }
        }

        [JsonIgnore]
        private string _amountWOVAT = "0";
        public string AmountWOVAT
        {
            get
            {
                return string.IsNullOrEmpty(_amountWOVAT) ? "0" : _amountWOVAT.Replace(",", ".");
            }
            set { _amountWOVAT = value; }
        }

        [JsonIgnore]
        private string _taxRate = "";
        public string TaxRate
        {
            get
            {
                return string.IsNullOrEmpty(_taxRate) ? "" : _taxRate;
            }
            set { _taxRate = value; }
        }

        [JsonIgnore]
        private string _VAT = "0";
        public string VAT
        {
            get
            {
                return string.IsNullOrEmpty(_VAT) ? "0" : _VAT.Replace(",", ".");
            }
            set { _VAT = value; }
        }

        [JsonIgnore]
        private string _amount = "0";
        public string Amount
        {
            get
            {
                return string.IsNullOrEmpty(_amount) ? "0" : _amount.Replace(",", ".");
            }
            set { _amount = value; }
        }
    }

    public enum FieldTypeEnum
    {
        Datetime,
        String,
        Double
    }
}
