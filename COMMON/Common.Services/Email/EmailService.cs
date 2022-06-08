// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;
using COMMON.Common.Services.StorageService;
using COMMON.Models;

namespace CloudArchive.Services
{
    public class EmailService : IEmailService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _cfg;
        private readonly IStorageService<StoredFile> _fileStorage;

        public EmailService(IWebHostEnvironment env, IConfiguration configuration, IStorageService<StoredFile> fileStorage)
        {
            _cfg = configuration;
            _env = env;
            _fileStorage = fileStorage;
        }

        public async Task<string> SendEmailAsync(List<string> emails, string subject, string message, List<DocFile> files = null)
        {
            try
            {
                if (string.IsNullOrEmpty(_cfg["SENDGRID_API_KEY"]))
                {
                    var mimeMess = new MailMessage();
                    foreach (string email in emails)
                    {
                        mimeMess.To.Add(new MailAddress(email));
                    }
                    mimeMess.From = new MailAddress(_cfg["EmailSettings_Sender"], _cfg["EmailSettings_SenderName"]);
                    mimeMess.Subject = subject;
                    mimeMess.Body = message;
                    mimeMess.IsBodyHtml = true;

                    SmtpClient client = new SmtpClient();
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential(_cfg["EmailSettings_Sender"], _cfg["EmailSettings_Password"]);
                    client.Port = int.Parse(_cfg["EmailSettings_MailPort"]);
                    client.Host = _cfg["EmailSettings_MailServer"];
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = true;
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            var binary = _fileStorage.GetFileAsync(file).GetAwaiter().GetResult();
                            using (MemoryStream filestr = new MemoryStream(binary))
                            {
                                System.Net.Mail.Attachment att = new System.Net.Mail.Attachment(filestr, file.FileName);
                                mimeMess.Attachments.Add(att);
                            }
                        }
                    }
                    try
                    {
                        client.Send(mimeMess);
                    }
                    catch (Exception ex)
                    {
                        return "Не удалось отправить почту. Ошибка: " + ex.Message + "StackTrace: " + ex.StackTrace;
                    }

                }
                else
                {
                    await SendTwilio(emails, subject, message, files);
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return "Не удалось отправить почту. Ошибка: " + ex.Message + "StackTrace: " + ex.StackTrace;
            }
        }
        private async Task<bool> SendTwilio(List<string> emails, string subject, string message, List<DocFile> files = null)
        {
            var apiKey = _cfg["SENDGRID_API_KEY"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_cfg["EmailSettings_Sender"], _cfg["EmailSettings_SenderName"]);
            List<EmailAddress> tos = new List<EmailAddress>();
            foreach (var email in emails)
            {
                tos.Add(new EmailAddress(email, email));
            }
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", message, false);
            if (files != null)
            {
                foreach (var file in files)
                {
                    var binary = _fileStorage.GetFileAsync(file).GetAwaiter().GetResult();
                    var attch = new SendGrid.Helpers.Mail.Attachment()
                    {
                        Content = Convert.ToBase64String(binary),
                        Filename = file.FileName,
                        Disposition = "inline",
                        ContentId = file.Id.ToString()
                    };
                    msg.AddAttachment(attch);
                }
            }
            try
            {
                var response = await client.SendEmailAsync(msg);
                if (!response.IsSuccessStatusCode)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
