// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.Servises;
using AutoMapper;
using CloudArchive.Services;
using CloudArchive.Services.PermissionService;
using COMMON.Common.Services.StorageService;
using COMMON.Models;
using DATABASE.Context;
using DATABASE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace COMMON.Common.Services.ContextService
{
    public interface IContextService
    {
        AppUser User { get; }
        ClientDTO ClientDto { get; }
        Client Client { get; }
        UserClient UserClient { get; }
        List<string> Roles { get; }
        Tariffs Tariff { get; }
        string Theme { get; }

        IAdminService DataBase { get; }
        ITokenService TokenService { get; }
        LinkGenerator LinkGenerator { get; }
        ICommonService CommonService { get; }
        IConfiguration Configuration { get; }
        IEmailService EmailSender { get; }
        SearchServiceDBContext DbContext { get; }
        UserManager<AppUser> UserManager { get; }
        IHttpContextAccessor Context { get; }
        IUIReadService DbRead { get; }
        IMapper Mapper { get; }
        SignInManager<AppUser> SignInManager { get; }
        IPermissionService PermService { get; }
        RoleManager<IdentityRole> RoleManager { get; }
        IDBWriteService DbWrite { get; }
        IUserService UserService { get; }
        IStorageService<StoredFile> FileStorage { get; }
        List<DocType> DocTypes { get; }
    }
}
