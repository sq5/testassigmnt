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
using ARCHIVE.COMMON.Servises;
using CloudArchive.Services;
using COMMON.Common.Services.ContextService;
using COMMON.Utilities;
using DATABASE.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace COMMON.Workflow
{
    public class Workflow
    {
        public static List<TaskTypeDefinition> TaskTypes
        {
            get
            {
                List<TaskTypeDefinition> _TaskTypeDefinitions = new List<TaskTypeDefinition>();
                _TaskTypeDefinitions.Add(new TaskTypeDefinition("Согласование", "Согласовано", "На согласовании", 0, " согласовал(а)"));
                _TaskTypeDefinitions.Add(new TaskTypeDefinition("Подписание", "Подписано", "На подписании", 1, " подписал(а)"));
                _TaskTypeDefinitions.Add(new TaskTypeDefinition("Исполнение", "Исполнено", "На исполнении", 2, " исполнил(а)"));
                _TaskTypeDefinitions.Add(new TaskTypeDefinition("Ознакомление", "Ознакомлено", "На ознакомлении", 3, " ознакомился(ась)"));
                _TaskTypeDefinitions.Add(new TaskTypeDefinition("Прерывание", "Прерывание", "Прерывание", 5, "Прерывание"));
                _TaskTypeDefinitions.Add(new TaskTypeDefinition("Старт", "Старт", "Старт", 4, " Старт"));
                return _TaskTypeDefinitions;
            }
        }

        private readonly IAdminService _db;
        private readonly SearchServiceDBContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppUser User;
        private readonly IConfiguration _cfg;
        private readonly ICommonService _commonService;
        private readonly IEmailService _emailSender;
        private readonly string _currentTheme;

        private UsersTasksDTO task;
        private List<UsersTasksDTO> _nextTasks;
        private List<UsersTasksDTO> _additionalTasks;
        private readonly List<UsersTasksDTO> _defectedTasks;
        private List<UsersTasksDTO> _declinedTasks;
        private UsersTasks _initiator;
        private MetadataDTO meta = null;
        private ContractDTO ctr = null;
        private string dispUsr, Doclink, DocName, Settname;
        TaskTypeDefinition ttype;

        public Workflow(IAdminService db,
            SearchServiceDBContext dbContext,
            UserManager<AppUser> userManager,
            ICommonService commonService,
            IEmailService emailSender,
            AppUser user,
            IConfiguration configuration)
        {
            _db = db;
            _dbContext = dbContext;
            _userManager = userManager;
            _commonService = commonService;
            _emailSender = emailSender;
            User = user;
            _cfg = configuration;
            _nextTasks = new List<UsersTasksDTO>();
            _additionalTasks = new List<UsersTasksDTO>();
            _defectedTasks = new List<UsersTasksDTO>();
            _declinedTasks = new List<UsersTasksDTO>();
        }

        public Workflow(IContextService context)
        {
            _db = context.DataBase;
            _dbContext = context.DbContext;
            _userManager = context.UserManager;
            _commonService = context.CommonService;
            _emailSender = context.EmailSender;
            User = context.User;
            _cfg = context.Configuration;
            _nextTasks = new List<UsersTasksDTO>();
            _additionalTasks = new List<UsersTasksDTO>();
            _defectedTasks = new List<UsersTasksDTO>();
            _declinedTasks = new List<UsersTasksDTO>();
            _currentTheme = context.Theme;
        }

        public Workflow(IContextService context, AppUser user)
        {
            _db = context.DataBase;
            _dbContext = context.DbContext;
            _userManager = context.UserManager;
            _commonService = context.CommonService;
            _emailSender = context.EmailSender;
            User = user;
            _cfg = context.Configuration;
            _nextTasks = new List<UsersTasksDTO>();
            _additionalTasks = new List<UsersTasksDTO>();
            _defectedTasks = new List<UsersTasksDTO>();
            _declinedTasks = new List<UsersTasksDTO>();
            _currentTheme = context.Theme;
        }

        public List<UsersTasksDTO> GetApprovalHistory(string sett, long id)
        {
            var res = new List<UsersTasksDTO>();
            try
            {
                var userTasks = sett != "Contracts" ? _dbContext.UsersTasks.AsNoTracking().Where(u => u.MetadataId == id) : _dbContext.UsersTasks.AsNoTracking().Where(u => u.ContractId == (int)id);
                res = (from tasks in userTasks
                       join usr in _dbContext.Users on tasks.Users equals usr.UserName into uj
                       from usr in uj.DefaultIfEmpty()
                       select new UsersTasksDTO
                       {
                           Id = tasks.Id,
                           Users = usr != null ? usr.DisplayName : tasks.Users,
                           DeadLine = tasks.DeadLine,
                           Active = tasks.Active,
                           TaskType = tasks.TaskType,
                           TaskText = tasks.TaskText,
                           StartDate = tasks.StartDate,
                           EndDate = tasks.EndDate,
                           Comment = tasks.Comment,
                           Resolution = tasks.Resolution,
                           Stage = tasks.Stage,
                           Order = tasks.Order,
                           Pictures = new List<byte[]>() { usr.Picture },
                           Position = usr.Position,
                           ApprovementType = tasks.ApprovementType
                       }
                       ).ToList();

                res = res.OrderBy(u => u.Stage).ThenBy(u => u.Order).ToList();
                var rows = new List<UsersTasksDTO>();
                if (res.Count > 0)
                {
                    UsersTasksDTO lastrow = res[0];
                    foreach (var row in res)
                    {
                        if (row.Order > lastrow.Order || row.Stage != lastrow.Stage)
                        {
                            rows.Add(lastrow);
                            lastrow = row;
                        }
                        else if (string.IsNullOrEmpty(row.Resolution) && row.AppType == ApprovementTypeEnum.Consecutive)
                        {
                            lastrow.Users += " или " + row.Users;
                            lastrow.Pictures = lastrow.Pictures.Concat(row.Pictures).ToList();
                        }
                        else if (row.AppType == ApprovementTypeEnum.Parallel)
                        {
                            rows.Add(lastrow);
                            lastrow = row;
                        }
                    }
                    rows.Add(lastrow);
                }
                return rows;
            }
            catch (Exception)
            {
                return res.ToList();
            }
        }

        public async Task<bool> StartProcess(string sett, long id, int TemplateId)
        {
            var Template = await GetWorkflowByTemplate(TemplateId);
            return await StartProcess(sett, id, Template);
        }

        public async Task<bool> StartProcess(string sett, long id, List<UsersTasksDTO> modelArr)
        {
            bool ret = false;
            try
            {
                MailConstructor mailer = new MailConstructor(_commonService, _emailSender, _cfg, _currentTheme);
                mailer.SetTemplate(MailTemplate.Approval_Task);
                string tasktype = modelArr.Count > 0 ? modelArr[0].TaskType : "";
                var ttype = TaskTypes.Where(x => x.TaskTypeName == tasktype).FirstOrDefault();
                var user = await _db.SingleAsync<UserClient, UserClient>(c => c.UserId.Equals(User.Id));
                var client = _dbContext.Clients.AsNoTracking().FirstOrDefault(c => c.Id == user.ClientId);

                if (sett == "Contracts")
                {
                    var ctr = Ensol.CommonUtils.Common.GetContractByID((int)id, _dbContext, client.Id);
                    if (ctr != null)
                    {
                        ctr.State = ttype.TaskDocState;
                        await _db.UpdateAsync<ContractDTO, Contract>(ctr);
                        mailer.FillContractFields(ctr);
                        DocName = ctr.DocType + " " + ctr.DocNumber + " " + ctr.DocDate?.ToString("dd.MM.yyyy") + " " + ctr.Contractor?.Name;
                    }
                }
                else
                {
                    var meta = Ensol.CommonUtils.Common.GetMetadataByID(id, _dbContext, client.Id);
                    if (meta != null)
                    {
                        meta.State = ttype.TaskDocState;
                        await _db.UpdateAsync<MetadataDTO, Metadata>(meta);
                        mailer.FillMetadataFields(meta, _dbContext);
                        DocName = meta.DocType + " " + meta.DocNumber + " " + meta.DocDate?.ToString("dd.MM.yyyy") + " " + meta?.Contractor?.Name;
                    }
                }
                var startProcess = new UsersTasksDTO();
                startProcess.Active = false;
                startProcess.Users = User.UserName;
                startProcess.TaskType = "Старт";
                startProcess.StartDate = DateTime.Now;
                startProcess.EndDate = DateTime.Now;
                startProcess.Resolution = "Старт процесса";
                startProcess.Order = 0;
                startProcess.ApprovementType = "Последовательный";
                startProcess.Created = DateTime.Now;
                if (sett == "Contracts")
                {
                    var coll = _dbContext.UsersTasks.AsNoTracking().Where(u => u.ContractId == (int)id);
                    startProcess.ContractId = (int)id;
                    startProcess.Stage = coll.Count() == 0 ? 1 : coll.Max(u => u.Stage) + 1;
                    startProcess.MetadataId = null;
                }
                else
                {
                    var coll = _dbContext.UsersTasks.AsNoTracking().Where(u => u.MetadataId == (int)id);
                    startProcess.MetadataId = id;
                    startProcess.Stage = coll.Count() == 0 ? 1 : coll.Max(u => u.Stage) + 1;
                    startProcess.ContractId = null;
                }
                var result = (await _db.CreateAsyncInt32<UsersTasksDTO, UsersTasks>(startProcess)) > 0;
                if (!result)
                    return result;
                int cntTasks = 0;
                foreach (UsersTasksDTO task in modelArr)
                {
                    if (cntTasks == 0)
                    {
                        task.Active = true;
                        task.StartDate = DateTime.Now;
                    }
                    else
                    {
                        task.Active = false;
                    }
                    if (sett == "Contracts")
                    {
                        task.ContractId = (int)id;
                        task.MetadataId = null;
                    }
                    else
                    {
                        task.ContractId = null;
                        task.MetadataId = id;
                    }
                    task.Order = task.Order * 10;
                    task.Stage = startProcess.Stage;
                    task.Created = DateTime.Now;
                    var users = task.Users;
                    var usersArr = users.Split(',');
                    foreach (var tuser in usersArr)
                    {
                        task.Users = _dbContext.Users.FirstOrDefault(u => u.Id == tuser)?.UserName;
                        if (task.Active)
                            await CheckSubstitute(task);
                        result = (await _db.CreateAsyncInt32<UsersTasksDTO, UsersTasks>(task)) > 0;
                        if (!result)
                            return result;
                        if (cntTasks == 0)
                        {
                            if (task.ContractId != null)
                            {
                                Doclink = "<a href='" + _cfg["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + task.ContractId + "&SettName=" + sett + "'>" + DocName + "</a>";
                                var nextText = "Документ " + Doclink + " поступил на " + task.TaskType;
                                await _commonService.CreateUserEvent(task.Users, nextText, DateTime.Now, task.ContractId.Value);
                            }
                            else
                            {
                                Doclink = "<a href='" + _cfg["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + task.MetadataId + "&SettName=" + sett + "'>" + DocName + "</a>";
                                var nextText = "Документ " + Doclink + " поступил на " + task.TaskType;
                                await _commonService.CreateUserEvent(task.Users, nextText, DateTime.Now, 0, (int)task.MetadataId.Value);
                            }
                            mailer.FillWFTaskFields(task);
                            var initiatorname = _dbContext.Users.FirstOrDefault(u => u.Email == startProcess.Users).DisplayName;
                            mailer.SetValue("%initiator%", initiatorname);
                            await mailer.SendMail("Поступил документ на " + task.TaskType.ToLower(), task.Users);
                        }
                    }
                    cntTasks++;
                }
                return true;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        public async Task<bool> EndProcess(string sett, long id, string Comment)
        {
            try
            {
                var user = await _db.SingleAsync<UserClient, UserClient>(c => c.UserId.Equals(User.Id));
                var client = _dbContext.Clients.AsNoTracking().FirstOrDefault(c => c.Id == user.ClientId);
                if (sett == "Contracts")
                {
                    var ctr = Ensol.CommonUtils.Common.GetContractByID((int)id, _dbContext, client.Id);
                    if (ctr != null)
                    {
                        ctr.State = "Прервано";
                        await _db.UpdateAsync<ContractDTO, Contract>(ctr);
                    }
                }
                else
                {
                    var meta = Ensol.CommonUtils.Common.GetMetadataByID(id, _dbContext, client.Id);
                    if (meta != null)
                    {
                        meta.State = "Прервано";
                        await _db.UpdateAsync<MetadataDTO, Metadata>(meta);
                    }
                }
                var activeTasks = sett == "Contracts" ? _dbContext.UsersTasks.AsNoTracking().Where(u => u.ContractId == (int)id).ToList() : _dbContext.UsersTasks.AsNoTracking().Where(u => u.MetadataId == (int)id).ToList();
                foreach (UsersTasks task in activeTasks)
                {
                    task.Active = false;
                    if (task.Resolution == null)
                        task.Resolution = "Прервано";
                    await _db.UpdateAsync<UsersTasks, UsersTasks>(task);
                }
                var startProcess = new UsersTasksDTO();
                startProcess.Active = false;
                startProcess.Users = User.UserName;
                startProcess.TaskType = "Прерывание";
                startProcess.StartDate = DateTime.Now;
                startProcess.EndDate = DateTime.Now;
                startProcess.Resolution = "Прерывание";
                startProcess.Created = DateTime.Now;
                startProcess.Comment = Comment;
                startProcess.ApprovementType = "Последовательный";
                if (sett == "Contracts")
                {
                    var coll = _dbContext.UsersTasks.AsNoTracking().Where(u => u.ContractId == (int)id);
                    startProcess.ContractId = (int)id;
                    startProcess.Stage = coll.Count() == 0 ? 0 : coll.Max(u => u.Stage);
                    startProcess.Order = coll.Count() == 0 ? 1 : coll.Max(u => u.Order) + 1;
                    startProcess.MetadataId = null;
                }
                else
                {
                    var coll = _dbContext.UsersTasks.AsNoTracking().Where(u => u.MetadataId == (int)id);
                    startProcess.MetadataId = id;
                    startProcess.Stage = coll.Count() == 0 ? 0 : coll.Max(u => u.Stage);
                    startProcess.Order = coll.Count() == 0 ? 1 : coll.Max(u => u.Order) + 1;
                    startProcess.ContractId = null;
                }
                var result = (await _db.CreateAsyncInt32<UsersTasksDTO, UsersTasks>(startProcess)) > 0;
                if (!result)
                    return result;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> EndProcess(string sett, long id)
        {
            return await EndProcess(sett, id, "");
        }

        public bool CheckUserPerformer(string sett, long id)
        {
            bool ret = false;
            try
            {
                var res = sett == "Contracts" ?
                _dbContext.UsersTasks.AsNoTracking().Where(u => u.ContractId == id && u.Users == User.UserName && u.Order == 0)?.OrderByDescending(x => x.Stage).FirstOrDefault() :
                _dbContext.UsersTasks.AsNoTracking().Where(u => u.MetadataId == (int)id && u.Users == User.UserName && u.Order == 0)?.OrderByDescending(x => x.Stage).FirstOrDefault();
                return res != null;
            }
            catch (Exception)
            {
                return ret;
                throw;
            }
        }

        public bool CheckUserCanStopProcess(string sett, long id)
        {
            bool ret = false;
            try
            {
                var startedByCurrUser = sett == "Contracts" ?
                _dbContext.UsersTasks.AsNoTracking().Where(u => u.ContractId == id && u.Users == User.UserName && u.Order == 0)?.OrderByDescending(x => x.Stage).FirstOrDefault() :
                _dbContext.UsersTasks.AsNoTracking().Where(u => u.MetadataId == (int)id && u.Users == User.UserName && u.Order == 0)?.OrderByDescending(x => x.Stage).FirstOrDefault();
                var hasActiveTask = sett == "Contracts" ?
                _dbContext.UsersTasks.AsNoTracking().Where(u => u.ContractId == id && u.Active == true)?.OrderByDescending(x => x.Stage).FirstOrDefault() :
                _dbContext.UsersTasks.AsNoTracking().Where(u => u.MetadataId == (int)id && u.Active == true)?.OrderByDescending(x => x.Stage).FirstOrDefault();
                return startedByCurrUser != null && hasActiveTask != null;
            }
            catch (Exception)
            {
                return ret;
                throw;
            }
        }

        public UserTask GetActiveTask(string sett, long id)
        {
            UserTask ret = new UserTask();
            try
            {
                var res = sett == "Contracts" ?
                    _dbContext.UsersTasks.AsNoTracking().FirstOrDefault(u => u.ContractId == id && u.Users == User.UserName && u.Active == true) :
                    _dbContext.UsersTasks.AsNoTracking().FirstOrDefault(u => u.MetadataId == (int)id && u.Users == User.UserName && u.Active == true);
                if (res == null)
                    return ret;
                ret.Id = res.Id;

                var ttype = TaskTypes.Where(x => x.TaskTypeName == res.TaskType).FirstOrDefault();
                ret.TaskType = ttype.TaskTypeNumber;
                return ret;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        private void CheckTask()
        {
            if (!task.Active)
            {
                throw new Exception("Данная задача была завершена ранее!");
            }
            if (User.Email != task.Users)
            {
                throw new Exception("Заадача назначена другому пользователю!");
            }
        }
        public async Task<string> TaskAgreement(TaskParams model)
        {
            try
            {
                // init variables
                task = await _db.SingleAsync<UsersTasks, UsersTasksDTO>(u => u.Id == model.Id);
                var usr = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserName == task.Users);
                var user = await _db.SingleAsync<UserClient, UserClient>(c => c.UserId.Equals(User.Id));
                var client = _dbContext.Clients.AsNoTracking().FirstOrDefault(c => c.Id == user.ClientId);
                dispUsr = usr == null ? "" : usr.DisplayName;
                var DocID = "";
                CheckTask();
                if (task.MetadataId > 0)
                {
                    meta = Ensol.CommonUtils.Common.GetMetadataByID(task.MetadataId.Value, _dbContext, client.Id);
                    DocName = meta.DocType + " " + meta.DocNumber + " " + meta.DocDate?.ToString("dd.MM.yyyy") + " " + meta?.Contractor?.Name;
                    DocID = meta.Id.ToString();
                    Settname = _dbContext.DocTypes.AsNoTracking().Where(x => x.Id == meta.DocTypeId).FirstOrDefault().Reestr;
                    if (meta.Deleted)
                    {
                        throw new Exception("Данный документ был удален!");
                    }
                    if (meta.EDIProvider == "КЭП" && model.Agreed && model.TaskType == 1 && !_dbContext.SignaturesAndEDIEvents.Where(x => x.MetaID == task.MetadataId && x.SignatureBin != null).Any())
                    {
                        throw new Exception("Данный документ помечен как ЭДО. Необходимо подписать файл электронной подписью!");
                    }
                }
                else if (task.ContractId > 0)
                {
                    ctr = Ensol.CommonUtils.Common.GetContractByID(task.ContractId.Value, _dbContext, client.Id);
                    string Contractor = ctr.Contractor == null ? "" : " контрагент " + ctr.Contractor?.Name;
                    DocName = "Договор " + ctr.DocNumber + Contractor;
                    DocID = ctr.Id.ToString();
                    Settname = _dbContext.DocTypes.AsNoTracking().Where(x => x.Id == ctr.DocTypeId).FirstOrDefault().Reestr;
                    if (ctr.Deleted)
                    {
                        throw new Exception("Данный документ был удален!");
                    }
                    if (ctr.EDIProvider == "КЭП" && model.Agreed
                    && model.TaskType == 1 && !_dbContext.SignaturesAndEDIEvents.Where(x => x.ContractID == task.ContractId
                    && x.SignatureBin != null).Any())
                    {
                        throw new Exception("Данный документ помечен как ЭДО. Необходимо подписать файл электронной подписью!");
                    }
                }
                //надо дописать логику что берется следующий по величине ордер
                _nextTasks = await _db.GetAsync<UsersTasks, UsersTasksDTO>(u => u.Order == task.Order + 10 && u.Stage == task.Stage && (task.MetadataId > 0 ? u.MetadataId == task.MetadataId : u.ContractId == task.ContractId), false);
                _additionalTasks = await _db.GetAsync<UsersTasks, UsersTasksDTO>(u => u.Order == task.Order && u.Stage == task.Stage && (task.MetadataId > 0 ? u.MetadataId == task.MetadataId : u.ContractId == task.ContractId) && u.Id != task.Id && u.Active, false);
                _initiator = _dbContext.UsersTasks.AsNoTracking().FirstOrDefault(u => u.Order == 0 && u.Stage == task.Stage && (task.MetadataId > 0 ? u.MetadataId == task.MetadataId : u.ContractId == task.ContractId));
                string tasktype = _nextTasks.Any() ? _nextTasks.FirstOrDefault().TaskType : "";
                Doclink = "<a href='" + _cfg["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + DocID + "&SettName=" + Settname + "'>" + DocName + "</a>";
                ttype = TaskTypes.Where(x => x.TaskTypeNumber == model.TaskType).FirstOrDefault();
                MailConstructor mailer = new MailConstructor(_commonService, _emailSender, _cfg, _currentTheme);

                if (!model.Agreed)
                    mailer.SetTemplate(MailTemplate.Approval_Decline);
                else if (!_nextTasks.Any())
                    mailer.SetTemplate(MailTemplate.Approval_Complete);
                else if (model.TaskType == 1)
                    mailer.SetTemplate(MailTemplate.Signing_Task);
                else
                    mailer.SetTemplate(MailTemplate.Approval_Task);

                if (_initiator != null)
                {
                    var iusr = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserName == task.Users);
                    mailer.SetValue("%initiator%", iusr.DisplayName);
                }
                //Logic
                task.Active = false;
                task.EndDate = DateTime.Now;
                task.Resolution = model.Agreed ? ttype.TaskResolution : "Отклонено";
                task.Comment = model.Comment;

                if (ctr != null)
                {
                    if (_initiator != null)
                    {
                        var type = model.Agreed ? ttype.MailSubject : "отклонил(а)";
                        string Text = dispUsr + " " + type + " ваш документ " + Doclink;
                        await _commonService.CreateUserEvent(_initiator.Users, Text, DateTime.Now, task.ContractId.Value);
                    }
                    mailer.FillContractFields(ctr);
                }
                else if (meta != null)
                {
                    if (_initiator != null)
                    {
                        var type = model.Agreed ? ttype.MailSubject : "отклонил(а)";
                        string Text = dispUsr + " " + type + " ваш документ " + Doclink;
                        await _commonService.CreateUserEvent(_initiator.Users, Text, DateTime.Now, 0, (int)task.MetadataId.Value);
                    }
                    mailer.FillMetadataFields(meta, _dbContext);
                }
                if (_additionalTasks.Any() && task.AppType == ApprovementTypeEnum.Parallel)
                {
                    await ProcessParallelStep(model, mailer);
                }
                else if (_nextTasks.Any() && model.Agreed)
                {
                    if (task.AppType == ApprovementTypeEnum.Parallel)
                    {
                        _declinedTasks = await _db.GetAsync<UsersTasks, UsersTasksDTO>(u => u.Order == task.Order && u.Stage == task.Stage && (task.MetadataId > 0 ? u.MetadataId == task.MetadataId : u.ContractId == task.ContractId) && u.Resolution == "Отклонено", false);
                        if (_declinedTasks.Any())
                        {
                            await ProcessLastStep(model, mailer);
                        }
                        else
                            await ProcessNextStep(model, mailer);
                    }
                    else
                        await ProcessNextStep(model, mailer);
                }
                else
                {
                    if (!model.Agreed && task.AppType == ApprovementTypeEnum.Parallel)
                    {
                        _declinedTasks.Add(task);
                    }
                    await ProcessLastStep(model, mailer);
                }

                if (_additionalTasks.Any() && task.AppType == ApprovementTypeEnum.Consecutive)
                {
                    for (int i = _additionalTasks.Count() - 1; i >= 0; i--)
                    {
                        var additionalTask = _additionalTasks[i];
                        await _db.DeleteAsync<UsersTasks>(x => x.Id == additionalTask.Id);
                    }
                }
                await _db.UpdateAsync<UsersTasksDTO, UsersTasks>(task);
                if (_defectedTasks != null)
                {
                    foreach (var nextTask in _defectedTasks)
                    {
                        Workflow wf = new Workflow(_db, _dbContext, _userManager, _commonService, _emailSender, User, _cfg);
                        TaskParams extramodel = new TaskParams();
                        model.Agreed = true;
                        model.Comment = "Пользователь был удален";
                        var ttype = Workflow.TaskTypes.Where(x => x.TaskTypeName == task.TaskType).FirstOrDefault();
                        model.TaskType = ttype.TaskTypeNumber;
                        model.Id = nextTask.Id;
                        if (nextTask.MetadataId > 0)
                        {
                            model.Sett = "Metadata";
                        }
                        else if (nextTask.ContractId > 0)
                        {
                            model.Sett = "Contracts";
                        }
                        await wf.TaskAgreement(model);
                    }
                }
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private async Task<bool> ProcessParallelStep(TaskParams model, MailConstructor mailer)
        {
            if (_initiator != null)
            {
                if (model.Agreed)
                {
                    if (model.TaskType == 1)
                        mailer.SetTemplate(MailTemplate.Signing_Approve);
                    else
                        mailer.SetTemplate(MailTemplate.Approval_Approve);
                }
                else
                    mailer.SetTemplate(MailTemplate.Parallel_Decline);
                mailer.SetValue("%executor%", dispUsr);
                mailer.SetValue("%comment%", model.Comment);
                if (model.Agreed)
                    await mailer.SendMail(dispUsr + " " + ttype.MailSubject + " Ваш документ", _initiator.Users);
                else
                    await mailer.SendMail(dispUsr + " отклонил(а) Ваш документ", _initiator.Users);
            }
            return true;
        }

        private async Task<bool> ProcessNextStep(TaskParams model, MailConstructor mailer)
        {

            var nexttype = TaskTypes.Where(x => x.TaskTypeName == _nextTasks.FirstOrDefault().TaskType).FirstOrDefault();
            if (ctr != null)
            {
                ctr.State = nexttype.TaskDocState;
                await _db.UpdateAsync<ContractDTO, Contract>(ctr);
            }
            else if (meta != null)
            {
                meta.State = nexttype.TaskDocState;
                await _db.UpdateAsync<MetadataDTO, Metadata>(meta);
            }
            var nextText = "Документ " + Doclink + " поступил на " + nexttype.TaskTypeName;

            foreach (var nextTask in _nextTasks)
            {
                nextTask.Active = true;
                nextTask.StartDate = DateTime.Now;
                await CheckSubstitute(nextTask);
                await _db.UpdateAsync<UsersTasksDTO, UsersTasks>(nextTask);
                var usr = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserName == nextTask.Users);
                if (!_dbContext.AppUsers.AsNoTracking().Where(d => d.UserId.Equals(usr.Id)).Any())
                {
                    _defectedTasks.Add(nextTask);
                }
                else
                {
                    if (ctr != null)
                    {
                        await _commonService.CreateUserEvent(nextTask.Users, nextText, DateTime.Now, task.ContractId.Value);
                    }
                    else if (meta != null)
                    {
                        await _commonService.CreateUserEvent(nextTask.Users, nextText, DateTime.Now, 0, (int)task.MetadataId.Value);
                    }

                    mailer.FillWFTaskFields(nextTask);
                    await mailer.SendMail("Поступил документ на " + nexttype.TaskTypeName.ToLower(), nextTask.Users);
                }
            }

            if (_initiator != null)
            {
                if (model.TaskType == 1)
                    mailer.SetTemplate(MailTemplate.Signing_Approve);
                else
                    mailer.SetTemplate(MailTemplate.Approval_Approve);
                mailer.SetValue("%executor%", dispUsr);
                await mailer.SendMail(dispUsr + " " + ttype.MailSubject + " Ваш документ", _initiator.Users);
            }
            return true;
        }

        private async Task<bool> ProcessLastStep(TaskParams model, MailConstructor mailer)
        {
            if (ctr != null)
            {
                ctr.State = (model.Agreed && !_declinedTasks.Any()) ? ttype.TaskResolution : "Отклонено";
                await _db.UpdateAsync<ContractDTO, Contract>(ctr);
                if (model.Agreed && !_declinedTasks.Any())
                {
                    string Text = "Ваш договор " + Doclink + " был успешно согласован/исполнен";
                    await _commonService.CreateUserEvent(_initiator.Users, Text, DateTime.Now, task.ContractId.Value);
                }
            }
            else if (meta != null)
            {
                meta.State = (model.Agreed && !_declinedTasks.Any()) ? ttype.TaskResolution : "Отклонено";
                await _db.UpdateAsync<MetadataDTO, Metadata>(meta);
                if (model.Agreed && !_declinedTasks.Any())
                {
                    string Text = "Ваш документ " + Doclink + " был успешно согласован/исполнен";
                    await _commonService.CreateUserEvent(_initiator.Users, Text, DateTime.Now, 0, (int)task.MetadataId.Value);
                }
            }

            foreach (var nextTask in _nextTasks)
            {
                await _db.UpdateAsync<UsersTasksDTO, UsersTasks>(nextTask);
            }
            foreach (var declinedTask in _declinedTasks)
            {
                await _db.UpdateAsync<UsersTasksDTO, UsersTasks>(declinedTask);
            }
            if (_initiator != null)
            {
                mailer.SetValue("%comment%", model.Comment);
                mailer.SetValue("%executor%", dispUsr);
                if (model.Agreed && !_declinedTasks.Any())
                {
                    mailer.SetTemplate(MailTemplate.Approval_Complete);
                    await mailer.SendMail("По вашему документу был успешно завершен процесс согласования/исполнения", _initiator.Users);
                }
                else if (_declinedTasks.Any())
                {
                    mailer.SetTemplate(MailTemplate.Parallel_Decline_Final);
                    await mailer.SendMail("Ваш документ был отклонен на параллельном этапе", _initiator.Users);
                }
                else
                {
                    mailer.SetTemplate(MailTemplate.Approval_Decline);
                    await mailer.SendMail(dispUsr + " отклонил(а) Ваш документ", _initiator.Users);
                }
            }
            return true;
        }

        public async Task<List<UsersTasksDTO>> GetWorkflowByTemplate(int TemplateID)
        {
            var template = await _db.SingleAsync<WFTemplates, WFTemplateDTO>(x => x.Id == TemplateID);
            return template.WF;
        }

        public async Task<List<WFTemplateDTO>> GetWorkflowTemplates(string sett, long id)
        {
            var user = await _db.SingleAsync<UserClient, UserClient>(c => c.UserId.Equals(User.Id));
            var client = _dbContext.Clients.AsNoTracking().FirstOrDefault(c => c.Id == user.ClientId);

            var Type = sett == "Contracts" ?
                _dbContext.Contracts.AsNoTracking().FirstOrDefault(u => u.Id == id).DocTypeId :
                _dbContext.Metadatas.AsNoTracking().FirstOrDefault(u => u.Id == (int)id).DocTypeId;

            var templates = await _db.GetAsync<WFTemplates, WFTemplateDTO>(x => x.ClientId == user.ClientId && x.DocTypeID == Type);
            return templates.ToList();
        }

        public async Task<List<WFTemplateDTO>> GetWorkflowTemplates()
        {
            var user = await _db.SingleAsync<UserClient, UserClient>(c => c.UserId.Equals(User.Id));
            var client = _dbContext.Clients.AsNoTracking().FirstOrDefault(c => c.Id == user.ClientId);

            var templates = await _db.GetAsync<WFTemplates, WFTemplateDTO>(x => x.ClientId == user.ClientId);
            return templates.ToList();
        }

        public async Task<UsersTasksDTO> CheckSubstitute(UsersTasksDTO NextTask)
        {
            var user = await _db.SingleAsync<UserClient, UserClient>(c => c.UserId.Equals(User.Id));
            var client = _dbContext.Clients.AsNoTracking().FirstOrDefault(c => c.Id == user.ClientId);
            var dt = DateTime.Now.Date;
            var subs = _dbContext.Substitutions.AsNoTracking().Where(x => x.ClientId == client.Id && x.StartDate.Date < dt && x.EndDate.Date > dt && x.User == NextTask.Users);
            if (subs.Any())
            {
                var sub = subs.FirstOrDefault();
                NextTask.SubstituteFor = NextTask.Users;
                NextTask.Users = sub.Substitute;
            }
            return NextTask;
        }

        public async Task<string> DelegateTask(TaskParams model)
        {
            try
            {
                var tasks = _dbContext.UsersTasks.Where(u => u.Id == model.Id);
                if (tasks.Any())
                {
                    var task = tasks.FirstOrDefault();
                    var usr = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserName == task.Users);
                    var user = await _db.SingleAsync<UserClient, UserClient>(c => c.UserId.Equals(User.Id));
                    var client = _dbContext.Clients.AsNoTracking().FirstOrDefault(c => c.Id == user.ClientId);
                    task.Active = false;
                    task.EndDate = DateTime.Now;
                    task.Resolution = "Делегировано";
                    task.Comment = model.Comment;

                    UsersTasks newtask = new UsersTasks();
                    newtask.Active = true;
                    newtask.ApprovementType = task.ApprovementType;
                    newtask.Comment = "";
                    newtask.ContractId = task.ContractId;
                    newtask.Created = DateTime.Now;
                    newtask.DeadLine = task.DeadLine;
                    newtask.MetadataId = task.MetadataId;
                    newtask.Stage = task.Stage;
                    newtask.StartDate = DateTime.Now;
                    newtask.TaskType = task.TaskType;
                    newtask.TaskText = task.TaskText;
                    newtask.Users = model.DelegatedTo;
                    await _dbContext.UsersTasks.AddAsync(newtask);
                    if (task.ApprovementType == "Параллельный")
                    {
                        newtask.Order = task.Order;
                    }
                    else
                    {
                        newtask.Order = task.Order;
                        task.Order = task.Order - 1;
                    }
                    MailConstructor mailer = new MailConstructor(_commonService, _emailSender, _cfg, _currentTheme);

                    var nexttype = TaskTypes.Where(x => x.TaskTypeName == newtask.TaskType).FirstOrDefault();
                    if (nexttype.TaskTypeNumber == 1)
                        mailer.SetTemplate(MailTemplate.Signing_Task);
                    else
                        mailer.SetTemplate(MailTemplate.Approval_Task);

                    mailer.SetValue("%taskText%", newtask.TaskText);
                    mailer.SetValue("%dueDate%", newtask.DeadLine == null ? "" : String.Format("{0:dd.MM.yyyy}", task.DeadLine.Value));
                    mailer.SetValue("%type%", newtask.TaskType.ToLower());

                    _initiator = _dbContext.UsersTasks.AsNoTracking().FirstOrDefault(u => u.Order == 0 && u.Stage == task.Stage && (task.MetadataId > 0 ? u.MetadataId == task.MetadataId : u.ContractId == task.ContractId));
                    if (_initiator != null)
                    {
                        var iusr = _dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserName == task.Users);
                        mailer.SetValue("%initiator%", iusr.DisplayName);
                    }
                    if (task.MetadataId > 0)
                    {
                        meta = Ensol.CommonUtils.Common.GetMetadataByID(task.MetadataId.Value, _dbContext, client.Id);
                        mailer.FillMetadataFields(meta, _dbContext);
                        Settname = _dbContext.DocTypes.AsNoTracking().Where(x => x.Id == meta.DocTypeId).FirstOrDefault().Reestr;
                        DocName = meta.DocType + " " + meta.DocNumber + " " + meta.DocDate?.ToString("dd.MM.yyyy") + " " + meta?.Contractor?.Name;
                        Doclink = "<a href='" + _cfg["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + task.MetadataId + "&SettName=" + Settname + "'>" + DocName + "</a>";
                        var nextText = "Документ " + Doclink + " поступил на " + task.TaskType;
                        await _commonService.CreateUserEvent(task.Users, nextText, DateTime.Now, 0, (int)task.MetadataId.Value);
                    }
                    else if (task.ContractId > 0)
                    {
                        ctr = Ensol.CommonUtils.Common.GetContractByID(task.ContractId.Value, _dbContext, client.Id);
                        mailer.FillContractFields(ctr);
                        Settname = _dbContext.DocTypes.AsNoTracking().Where(x => x.Id == ctr.DocTypeId).FirstOrDefault().Reestr;
                        DocName = ctr.DocType + " " + ctr.DocNumber + " " + ctr.DocDate?.ToString("dd.MM.yyyy") + " " + ctr.Contractor?.Name;
                        Doclink = "<a href='" + _cfg["HttpClient_Address"] + "/newstyle/document/view?ItemId=" + task.ContractId + "&SettName=" + Settname + "'>" + DocName + "</a>";
                        var nextText = "Документ " + Doclink + " поступил на " + task.TaskType;
                        await _commonService.CreateUserEvent(task.Users, nextText, DateTime.Now, task.ContractId.Value);
                    }
                    await mailer.SendMail("Поступил документ на " + nexttype.TaskTypeName.ToLower(), newtask.Users);
                    _dbContext.SaveChanges();
                }
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
