// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.DTOModels.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using CloudArchive.Services;
using COMMON.Utilities;
using System.Threading.Tasks;

namespace CloudArchive.ScheduledTasks
{
    public class IMAPService : IIMAPService
    {
        private readonly ExtConnectionDTO _extConnectionDTO;
        private readonly IBackgroundServiceLog _backgroundServiceLog;
        private readonly IEmailService _emailSender;
        private readonly ClientDTO _client;
        private readonly ICommonService _commonService;
        private readonly CultureInfo _culture = new CultureInfo("ru-RU");

        public IMAPService(ExtConnectionDTO extConnectionDTO, IBackgroundServiceLog backgroundServiceLog, ClientDTO client, ICommonService commonService, IEmailService emailSender)
        {
            _extConnectionDTO = extConnectionDTO;
            _backgroundServiceLog = backgroundServiceLog;
            _client = client;
            _commonService = commonService;
            _emailSender = emailSender;
        }

        public async Task<bool> Process(List<string> AllUsersEmails)
        {
            using (var client = new ImapClient())
            {
                MailConstructor mailer = new MailConstructor(_commonService, _emailSender);
                mailer.SetTemplate(MailTemplate.IMAPErrorNotif);
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(_extConnectionDTO.Server, _extConnectionDTO.Port, SecureSocketOptions.SslOnConnect);
                client.Authenticate(_extConnectionDTO.User, _extConnectionDTO.Password);
                //var mail = _commonService.ReadFile("./wwwroot/src/EmailTemplates/IMAPErrorNotif.html");
                var inbox = client.GetFolder(_extConnectionDTO.FolderIn);
                if (inbox == null)
                    return false;
                inbox.Open(FolderAccess.ReadWrite);
                var folderError = client.GetFolder(_extConnectionDTO.FolderError);
                if (folderError == null)
                    return false;
                var folderProcessing = client.GetFolder(_extConnectionDTO.FolderProcessed);
                if (folderProcessing == null)
                    return false;
                int index = inbox.Count > 10 ? 9 : inbox.Count - 1;
                if (inbox.Count > 0)
                    _backgroundServiceLog.AddInfo($"Found {inbox.Count} in Server Name= {_extConnectionDTO.Server};Box Name= {_extConnectionDTO.User};Folder= {inbox}", "IMAPBackgroundService", _client.Id);
                foreach (var summary in inbox.Fetch(0, index, MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure | MessageSummaryItems.UniqueId))
                {
                    try
                    {
                        //var summary = inbox.Fetch(sum.Index, sum.Index - 1, MessageSummaryItems.UniqueId | MessageSummaryItems.Full | MessageSummaryItems.BodyStructure)[0];
                        var recdate = !summary.Envelope.Date.HasValue ? "" : summary.Envelope.Date.Value.ToString("dd.MM.yyyy HH:mm", _culture);
                        string sender = summary.Envelope.From.Mailboxes.FirstOrDefault()?.Address.ToLower();
                        string reciever = summary.Envelope.To.Mailboxes.FirstOrDefault()?.Address;
                        List<NonFormDocsDTO> nonFormDocsArr = new List<NonFormDocsDTO>();
                        var PDFParts = summary.BodyParts.Where(x => (x.ContentType.IsMimeType("application", "pdf") || (x.FileName != null && x.FileName.EndsWith(".pdf", true, null))));
                        if (PDFParts == null || PDFParts.Count() == 0)
                        {
                            inbox.MoveTo(summary.UniqueId, folderError);
                            _backgroundServiceLog.AddError("Не найден PDF в письме " + summary.NormalizedSubject, "IMAPBackgroundService", _extConnectionDTO.ClientId);
                            if (AllUsersEmails.Contains(sender))
                            {
                                mailer.SetValue("%SUBJ%", summary.NormalizedSubject);
                                mailer.SetValue("%Reciever%", reciever);
                                mailer.SetValue("%Recdate%", recdate);
                                await mailer.SendMail("Не удалось загрузить Ваши документы", sender);
                            }
                        }
                        else
                        {
                            foreach (var attachment in PDFParts)
                            {
                                try
                                {
                                    NonFormDocsDTO nonFormDocsDTO = new NonFormDocsDTO();
                                    nonFormDocsDTO.Binaries = new List<BinariesDTO>();
                                    nonFormDocsDTO.RequestID = Guid.NewGuid().ToString("d");
                                    nonFormDocsDTO.Modified = DateTime.Now;
                                    nonFormDocsDTO.Created = DateTime.Now;
                                    nonFormDocsDTO.ClientId = _client.Id;
                                    nonFormDocsDTO.Sender = sender;
                                    nonFormDocsDTO.OrganizationId = _extConnectionDTO.OrganizationId;
                                    nonFormDocsDTO.OCRSplit = _extConnectionDTO.OCRSplit;
                                    if (_extConnectionDTO.OCR.HasValue && _extConnectionDTO.OCR.Value)
                                    {
                                        nonFormDocsDTO.OCRState = "Отправка на распознавание";
                                    }
                                    var entity = inbox.GetBodyPart(summary.UniqueId, attachment);
                                    var part = (MimePart)entity;
                                    using (var memory = new MemoryStream())
                                    {
                                        part.Content.DecodeTo(memory);
                                        memory.Position = 0;
                                        byte[] PDF = memory.ToArray();
                                        var fileSizeInMb = (float)Math.Round((PDF.Length / 1024.0F) / 1024.0F, 2);
                                        if (fileSizeInMb > 20)
                                        {
                                            _backgroundServiceLog.AddError("IMAPBackgroundService. Файл " + attachment.FileName + " превышает 20Мб " + summary.NormalizedSubject, "IMAPBackgroundService", _client.Id);
                                            inbox.MoveTo(summary.UniqueId, folderError);
                                            if (AllUsersEmails.Contains(sender))
                                            {

                                                mailer.SetValue("%SUBJ%", summary.NormalizedSubject);
                                                mailer.SetValue("%Reciever%", reciever);
                                                mailer.SetValue("%Recdate%", recdate);
                                                await mailer.SendMail("Не удалось загрузить Ваши документы", sender);
                                            }
                                            continue;
                                        }
                                        nonFormDocsDTO.Binaries.Add(new BinariesDTO()
                                        {
                                            MetaId = 0,
                                            ContractId = 0,
                                            Created = DateTime.Now,
                                            Modified = DateTime.Now,
                                            FileSize = PDF.Length,
                                            FileName = !string.IsNullOrEmpty(attachment.ContentType?.Name) ? attachment.ContentType.Name : attachment.FileName,
                                            FileBase64 = Convert.ToBase64String(PDF)
                                        });
                                    }
                                    nonFormDocsArr.Add(nonFormDocsDTO);
                                }
                                catch (Exception ex)
                                {
                                    _backgroundServiceLog.AddError("IMAPBackgroundService. Не удалось обработать Письмо " + summary.NormalizedSubject + " ошибка " + ex, "IMAPBackgroundService", _client.Id);
                                    inbox.MoveTo(summary.UniqueId, folderError);
                                    if (AllUsersEmails.Contains(sender))
                                    {

                                        mailer.SetValue("%SUBJ%", summary.NormalizedSubject);
                                        mailer.SetValue("%Reciever%", reciever);
                                        mailer.SetValue("%Recdate%", recdate);
                                        await mailer.SendMail("Не удалось загрузить Ваши документы", sender);
                                    }
                                }
                            }
                            var success = _commonService.CreateNonFormDocs(nonFormDocsArr);
                            if (success.Result)
                            {
                                inbox.MoveTo(summary.UniqueId, folderProcessing);
                                _backgroundServiceLog.AddInfo("Успешно обработано письмо, тема: " + summary.NormalizedSubject, "IMAPBackgroundService", _client.Id);
                            }
                            else
                            {
                                _backgroundServiceLog.AddError("IMAPBackgroundService. Перемещено в FolderError" + summary.NormalizedSubject, "IMAPBackgroundService", _client.Id);
                                inbox.MoveTo(summary.UniqueId, folderError);
                                if (AllUsersEmails.Contains(sender))
                                {

                                    mailer.SetValue("%SUBJ%", summary.NormalizedSubject);
                                    mailer.SetValue("%Reciever%", reciever);
                                    mailer.SetValue("%Recdate%", recdate);
                                    await mailer.SendMail("Не удалось загрузить Ваши документы", sender);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        inbox.MoveTo(summary.UniqueId, folderError);
                        _backgroundServiceLog.AddError("Ошибка обработки письма, Перемещено в FolderError." + " Exception: " + ex.Message + ". Stacktrace: " + ex.StackTrace, "IMAPBackgroundService", _client.Id);
                    }

                }
                return true;
            }
        }
    }
}
