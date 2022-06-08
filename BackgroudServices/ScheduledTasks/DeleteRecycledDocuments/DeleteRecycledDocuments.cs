// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ARCHIVE.COMMON.DTOModels;
using DATABASE.Context;
using CloudArchive.Services;
using BackgroudServices.Scheduling;
using COMMON.Models;
using COMMON.Common.Services.StorageService;

namespace CloudArchive.ScheduledTasks
{
    public class DeleteRecycledDocumentsService : IScheduledTask
    {
        public string ServiceName { get => "DeleteRecycledDocumentsService"; }
        private static readonly int s_daysOffset = -50;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private bool error = false;

        public DeleteRecycledDocumentsService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IBackgroundServiceLog _backgroundServiceLog = scope.ServiceProvider.GetRequiredService<IBackgroundServiceLog>();
                IAdminService _adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
                SearchServiceDBContext _dbContext = scope.ServiceProvider.GetRequiredService<SearchServiceDBContext>();
                IStorageService<StoredFile> _fileStorage = scope.ServiceProvider.GetRequiredService<IStorageService<StoredFile>>();
                IQueryable<Metadata> meta = _dbContext.Metadatas.Where(x => x.Deleted && x.DeleteDate < DateTime.Now.AddDays(s_daysOffset));
                foreach (var data in meta)
                {
                    try
                    {
                        error = false;
                        var tasks = _dbContext.UsersTasks.Where(t => t.MetadataId == data.Id);
                        var favorites = _dbContext.Favorites.Where(t => t.MetaId == data.Id);
                        var versions = _dbContext.Versions.Where(t => t.MetadataId == data.Id);
                        var sigandevents = _dbContext.SignaturesAndEDIEvents.Where(t => t.MetaID == data.Id);
                        var addfields = _dbContext.AdditionalFields.Where(t => t.MetaId == data.Id);
                        _dbContext.RemoveRange(addfields);
                        _dbContext.RemoveRange(tasks);
                        _dbContext.RemoveRange(favorites);
                        _dbContext.RemoveRange(versions);
                        _dbContext.RemoveRange(sigandevents);
                        _dbContext.SaveChanges();

                        var files = await _adminService.GetAsync<DocFile, BinariesDTO>(f => f.MetaId.Equals(data.Id));
                        if (files.Count > 0)
                        {
                            foreach (BinariesDTO file in files)
                            {
                                var success = await _fileStorage.DeleteAsync(file);
                                _backgroundServiceLog.AddInfo($"DeleteRecycledDocuments. Delete file from Metadata  ={file.Id}", "DeleteRecycledDocuments", data.ClientId.Value);
                                if (!success)
                                {
                                    _backgroundServiceLog.AddError("DeleteRecycledDocuments. Не удалось удалить файл " + file.Id, "DeleteRecycledDocuments", data.ClientId.Value);
                                    error = true;
                                }
                            }
                        }
                        var result = (await _adminService.DeleteAsync<Metadata>(x => x.Id == data.Id));
                        if (!result.res)
                            _backgroundServiceLog.AddError("DeleteRecycledDocuments. Не удалось удалить документ " + data.Id + " : " + result.err, "DeleteRecycledDocuments", data.ClientId.Value);
                        else
                            _backgroundServiceLog.AddInfo("Удален документ <Metadata " + data.Id + " " + data.DocType + "N " + data.DocNumber, "DeleteRecycledDocuments", data.ClientId.Value);
                    }
                    catch (Exception ex)
                    {
                        _backgroundServiceLog.AddError("DeleteRecycledDocuments. Не удалось удалить документ " + data.Id + " : " + ex, "DeleteRecycledDocuments", data.ClientId.Value);
                    }
                }
                IQueryable<Contract> contr = _dbContext.Contracts.Where(x => x.Deleted && x.DeleteDate < DateTime.Now.AddDays(s_daysOffset));
                foreach (var data in contr)
                {
                    try
                    {
                        error = false;
                        var tasks = _dbContext.UsersTasks.Where(t => t.ContractId == data.Id);
                        var favorites = _dbContext.Favorites.Where(t => t.ContractId == data.Id);
                        var versions = _dbContext.Versions.Where(t => t.ContractId == data.Id);
                        var sigandevents = _dbContext.SignaturesAndEDIEvents.Where(t => t.ContractID == data.Id);
                        var addfields = _dbContext.AdditionalFields.Where(t => t.ContractId == data.Id);
                        _dbContext.RemoveRange(addfields);
                        _dbContext.RemoveRange(sigandevents);
                        _dbContext.RemoveRange(tasks);
                        _dbContext.RemoveRange(favorites);
                        _dbContext.RemoveRange(versions);
                        _dbContext.SaveChanges();
                        var files = await _adminService.GetAsync<DocFile, BinariesDTO>(f => f.ContractId.Equals(data.Id));
                        if (files.Count > 0)
                        {
                            foreach (BinariesDTO file in files)
                            {
                                var success = await _fileStorage.DeleteAsync(file);
                                _backgroundServiceLog.AddInfo($"DeleteRecycledDocuments. Delete file from Contracts =  {file.Id}", "DeleteRecycledDocuments", data.ClientId.Value);
                                if (!success)
                                {
                                    _backgroundServiceLog.AddError("DeleteRecycledDocuments. Не удалось удалить файл " + file.Id, "DeleteRecycledDocuments", data.ClientId.Value);
                                    error = true;
                                }
                            }
                        }
                        var result = (await _adminService.DeleteAsync<Contract>(x => x.Id == data.Id));
                        if (!result.res)
                            _backgroundServiceLog.AddError("DeleteRecycledDocuments. Не удалось удалить договор " + data.Id + " : " + result.err, "DeleteRecycledDocuments", data.ClientId.Value);
                        else
                            _backgroundServiceLog.AddInfo("Удален документ <Contract " + data.Id + " " + "N " + data.DocNumber, "DeleteRecycledDocuments", data.ClientId.Value);
                    }
                    catch (Exception ex)
                    {
                        _backgroundServiceLog.AddError("DeleteRecycledDocuments. Не удалось удалить договор " + data.Id + " : " + ex, "DeleteRecycledDocuments", data.ClientId.Value);
                    }
                }
                IQueryable<NonFormDocs> nfd = _dbContext.NonFormDocs.Where(x => (bool)x.Deleted && x.DeleteDate < DateTime.Now.AddDays(s_daysOffset));
                foreach (var data in nfd)
                {
                    try
                    {
                        error = false;
                        var files = await _adminService.GetAsync<DocFile, BinariesDTO>(f => f.NonFormDocId.Equals(data.Id));
                        if (files.Count > 0)
                        {
                            foreach (BinariesDTO file in files)
                            {
                                var success = await _fileStorage.DeleteAsync(file);
                                _backgroundServiceLog.AddInfo($"DeleteRecycledDocuments. Delete file from NonFormDoc  ={file.Id}", "DeleteRecycledDocuments", data.ClientId.Value);
                                if (!success)
                                {
                                    _backgroundServiceLog.AddInfo("DeleteRecycledDocuments. Не удалось удалить файл " + file.Id, "DeleteRecycledDocuments", data.ClientId.Value);
                                    error = true;
                                }
                            }
                        }
                        var result = (await _adminService.DeleteAsync<NonFormDocs>(x => x.Id == data.Id));
                        if (!result.res)
                            _backgroundServiceLog.AddInfo("DeleteRecycledDocuments. Не удалось удалить документ " + data.Id + " : " + result.err, "DeleteRecycledDocuments", data.ClientId.Value);
                        else
                            _backgroundServiceLog.AddInfo("Удален документ Nonform " + data.Id, "DeleteRecycledDocuments", data.ClientId.Value);
                    }
                    catch (Exception ex)
                    {
                        _backgroundServiceLog.AddError("DeleteRecycledDocuments. Не удалось удалить Nonform " + data.Id + " : " + ex, "DeleteRecycledDocuments", data.ClientId.Value);
                    }
                }
            }
        }
    }
}
