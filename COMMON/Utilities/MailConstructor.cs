// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using CloudArchive.Services;
using COMMON.Common.Services.ContextService;
using DATABASE.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace COMMON.Utilities
{
    public class MailConstructor
    {
        private Dictionary<string, string> MailVariables;
        private Dictionary<MailTemplate, string> MailTemplates;

        private List<string> _Recipients;
        private string _Subject;
        private string _Text;
        private string _Template;
        private string _theme;
        private List<DocFile> _files;
        private readonly IConfiguration _cfg;
        private readonly ICommonService _commonService;
        private readonly IEmailService _emailSender;
        public MailConstructor(IContextService context)
        {
            _commonService = context.CommonService;
            _emailSender = context.EmailSender;
            _cfg = context.Configuration;
            ClearValues();
        }

        public MailConstructor(ICommonService commonService, IEmailService emailSender, IConfiguration configuration = null, string theme = "")
        {
            _commonService = commonService;
            _emailSender = emailSender;
            _theme = theme;
            if (configuration != null)
                _cfg = configuration;
            ClearValues();
        }

        public void FillContractFields(ContractDTO contract)
        {
            SetValue("%numDoc%", contract.DocNumber);
            SetValue("%dateDoc%", contract.DocDate == null ? "" : String.Format("{0:dd.MM.yyyy}", contract.DocDate.Value));
            SetValue("%summ%", contract.Amount?.ToString("## ###.00"));
            SetValue("%contragent%", contract.Contractor == null ? "" : contract.Contractor.Name);
            SetValue("%DocType%", contract.DocType);
            SetValue("%DocKind%", contract.DocKind == null ? "" : contract.DocKind.Name);
            SetValue("%DocComment%", contract.Comment);
            if (_cfg != null)
            {
                SetValue("%link%", _cfg["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + contract.Id.ToString() + "&SettName=Contracts");
            }
        }

        public void FillMetadataFields(MetadataDTO meta, SearchServiceDBContext _db)
        {
            SetValue("%numDoc%", meta.DocNumber);
            SetValue("%dateDoc%", meta.DocDate == null ? "" : String.Format("{0:dd.MM.yyyy}", meta.DocDate.Value));
            SetValue("%summ%", meta.AmountToPay == null ? meta.Amount?.ToString("## ###.00") : meta.AmountToPay?.ToString("## ###.00"));
            SetValue("%contragent%", meta.Contractor == null ? "" : meta.Contractor.Name);
            SetValue("%DocType%", meta.DocType);
            SetValue("%DocKind%", meta.DocKind == null ? "" : meta.DocKind.Name);
            SetValue("%paymentDate%", meta.PaymentDate == null ? "" : String.Format("{0:dd.MM.yyyy}", meta.PaymentDate.Value));
            SetValue("%paymentNumber%", meta.PaymentNumber);
            SetValue("%DocComment%", meta.Comment);
            if (_cfg != null)
            {
                var Settname = _db.DocTypes.AsNoTracking().Where(x => x.Id == meta.DocTypeId).FirstOrDefault().Reestr;
                SetValue("%link%", _cfg["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + meta.Id.ToString() + "&SettName=" + Settname);
            }
        }

        public void FillWFTaskFields(UsersTasksDTO task)
        {
            SetValue("%taskText%", task.TaskText);
            SetValue("%dueDate%", task.DeadLine == null ? "" : String.Format("{0:dd.MM.yyyy}", task.DeadLine.Value));
            SetValue("%type%", task.TaskType.ToLower());
        }
        public void SetTemplate(MailTemplate template)
        {
            _Template = _commonService.ReadFile(MailTemplates[template]);
        }
        public void AddFile(DocFile _file)
        {
            _files.Add(_file);
        }

        public void SetTemplate(string templatePath)
        {
            _Template = _commonService.ReadFile(templatePath);
        }

        private void UpdateTemplate()
        {
            _Text = _Template;
            SetValue("%USERNAME%", _commonService.UserService.GetUserByEmailAsync(_Recipients[0]).GetAwaiter().GetResult()?.DisplayName);
            foreach (var pair in MailVariables)
            {
                _Text = _Text.Replace(pair.Key, pair.Value);
            }

        }

        public async Task<bool> SendMail()
        {
            UpdateTemplate();
            await _emailSender.SendEmailAsync(_Recipients, _Subject, _Text, _files);
            NewMail();
            return true;
        }

        public async Task<bool> SendMail(string subject)
        {
            _Subject = subject;
            await SendMail();
            return true;
        }

        public async Task<bool> SendMail(string subject, string user)
        {
            _Subject = subject;
            _Recipients.Add(user);
            await SendMail();
            return true;
        }

        public async Task<bool> SendMail(string subject, List<string> users)
        {
            _Subject = subject;
            _Recipients = users;
            await SendMail();
            return true;
        }
        public async Task<bool> SendMail(string subject, string body, List<string> users)
        {
            _Subject = subject;
            _Recipients = users;
            _Text = body;
            await _emailSender.SendEmailAsync(_Recipients, _Subject, _Text, _files);
            return true;
        }
        public void NewMail()
        {
            _Recipients = new List<string>();
            _Subject = "";
            _Text = "";
            _files = new List<DocFile>();
        }

        public void ClearValues()
        {
            NewMail();
            SetDictionaries();
        }

        public void SetValue(string Name, string Value)
        {
            if (MailVariables.ContainsKey(Name))
            {
                MailVariables[Name] = Value;
            }
            else
            {
                MailVariables.Add(Name, Value);
            }
        }

        private void SetDictionaries()
        {
            MailVariables = new Dictionary<string, string>();
            MailVariables.Add("%numDoc%", "");
            MailVariables.Add("%dateDoc%", "");
            MailVariables.Add("%DocType%", "");
            MailVariables.Add("%DocKind%", "");
            MailVariables.Add("%DocComment%", "");
            MailVariables.Add("%summ%", "");
            MailVariables.Add("%contragent%", "");
            MailVariables.Add("%link%", "");
            MailVariables.Add("%dueDate%", "");
            MailVariables.Add("%taskText%", "");
            MailVariables.Add("%type%", "");
            MailVariables.Add("%executor%", "");
            MailVariables.Add("%initiator%", "");


            MailTemplates = new Dictionary<MailTemplate, string>();
            MailTemplates.Add(MailTemplate.Approval_Approve, "./wwwroot/src/EmailTemplates/Approval_Approve.html");
            MailTemplates.Add(MailTemplate.Approval_Complete, "./wwwroot/src/EmailTemplates/Approval_Complete.html");
            MailTemplates.Add(MailTemplate.Approval_Decline, "./wwwroot/src/EmailTemplates/Approval_Decline.html");
            MailTemplates.Add(MailTemplate.Approval_Task, "./wwwroot/src/EmailTemplates/Approval_Task.html");
            MailTemplates.Add(MailTemplate.CreateClient, "./wwwroot/src/EmailTemplates/CreateClient.html");
            MailTemplates.Add(MailTemplate.CreateUserAndClient, "./wwwroot/src/EmailTemplates/CreateUserAndClient.html");
            MailTemplates.Add(MailTemplate.IncomigInvoicePaid, "./wwwroot/src/EmailTemplates/IncomigInvoicePaid.html");
            MailTemplates.Add(MailTemplate.NonFormDocApproval, "./wwwroot/src/EmailTemplates/NonFormDocApproval.html");
            MailTemplates.Add(MailTemplate.NonFormDocDecline, "./wwwroot/src/EmailTemplates/NonFormDocDecline.html");
            MailTemplates.Add(MailTemplate.OutgoingInvoiceCreated, "./wwwroot/src/EmailTemplates/OutgoingInvoiceCreated.html");
            MailTemplates.Add(MailTemplate.OutgoingInvoicePaid, "./wwwroot/src/EmailTemplates/OutgoingInvoicePaid.html");
            MailTemplates.Add(MailTemplate.ResetPassword, "./wwwroot/src/EmailTemplates/ResetPassword.html");
            MailTemplates.Add(MailTemplate.ResetPasswordAddUser, "./wwwroot/src/EmailTemplates/ResetPasswordAddUser.html");
            MailTemplates.Add(MailTemplate.Signing_Approve, "./wwwroot/src/EmailTemplates/Signing_Approve.html");
            MailTemplates.Add(MailTemplate.Signing_Task, "./wwwroot/src/EmailTemplates/Signing_Task.html");
            MailTemplates.Add(MailTemplate.BackUp, "./wwwroot/src/EmailTemplates/BackUp.html");
            MailTemplates.Add(MailTemplate.IMAPErrorNotif, "./wwwroot/src/EmailTemplates/IMAPErrorNotif.html");
            MailTemplates.Add(MailTemplate.BillPaymentNotif, "./wwwroot/src/EmailTemplates/BillPaymentNotif.html");
            MailTemplates.Add(MailTemplate.WFMyExpiredDocs, "./wwwroot/src/EmailTemplates/WFMyExpiredDocs.html");
            MailTemplates.Add(MailTemplate.WFMyTasks, "./wwwroot/src/EmailTemplates/WFMyTasks.html");
            MailTemplates.Add(MailTemplate.Parallel_Decline, "./wwwroot/src/EmailTemplates/Parallel_Decline.html");
            MailTemplates.Add(MailTemplate.Parallel_Decline_Final, "./wwwroot/src/EmailTemplates/Parallel_Decline_Final.html");
            MailTemplates.Add(MailTemplate.ContractExpireNotify, "./wwwroot/src/EmailTemplates/ContractExpireNotify.html");
        }
    }

    public enum MailTemplate
    {
        Approval_Approve,
        ContractExpireNotify,
        Approval_Complete,
        Approval_Decline,
        Approval_Task,
        CreateClient,
        CreateUserAndClient,
        IncomigInvoicePaid,
        NonFormDocApproval,
        NonFormDocDecline,
        OutgoingInvoiceCreated,
        OutgoingInvoicePaid,
        ResetPassword,
        ResetPasswordAddUser,
        Signing_Approve,
        Signing_Task,
        BackUp,
        IMAPErrorNotif,
        BillPaymentNotif,
        WFMyExpiredDocs,
        WFMyTasks,
        Parallel_Decline,
        Parallel_Decline_Final
    }
}
