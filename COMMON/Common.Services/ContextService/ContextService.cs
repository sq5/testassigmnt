// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace COMMON.Common.Services.ContextService
{
    public class ContextService : IContextService
    {
        private readonly IAdminService _db;
        public IAdminService DataBase { get { return _db; } }

        private readonly ITokenService _tokenService;
        public ITokenService TokenService { get { return _tokenService; } }

        private readonly LinkGenerator _linkGenerator;
        public LinkGenerator LinkGenerator { get { return _linkGenerator; } }

        private readonly ICommonService _commonService;
        public ICommonService CommonService { get { return _commonService; } }

        private readonly IConfiguration _configuration;
        public IConfiguration Configuration { get { return _configuration; } }

        private readonly IEmailService _emailSender;
        public IEmailService EmailSender { get { return _emailSender; } }

        private readonly SearchServiceDBContext _dbContext;
        public SearchServiceDBContext DbContext { get { return _dbContext; } }

        private readonly UserManager<AppUser> _userManager;
        public UserManager<AppUser> UserManager { get { return _userManager; } }

        private readonly IHttpContextAccessor _context;
        public IHttpContextAccessor Context { get { return _context; } }

        private readonly IUIReadService _dbRead;
        public IUIReadService DbRead { get { return _dbRead; } }

        private readonly IMapper _mapper;
        public IMapper Mapper { get { return _mapper; } }


        private readonly SignInManager<AppUser> _signInManager;
        public SignInManager<AppUser> SignInManager { get { return _signInManager; } }

        private readonly IPermissionService _permService;
        public IPermissionService PermService { get { return _permService; } }

        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleManager<IdentityRole> RoleManager { get { return _roleManager; } }

        private readonly IDBWriteService _write;
        public IDBWriteService DbWrite { get { return _write; } }

        private readonly IUserService _userService;
        public IUserService UserService { get { return _userService; } }

        private readonly IStorageService<StoredFile> _fileStorage;
        public IStorageService<StoredFile> FileStorage { get { return _fileStorage; } }

        public ContextService(UserManager<AppUser> userManager, IHttpContextAccessor context, IUIReadService read, IMapper mapper, IAdminService db, ITokenService tokenService, IDBWriteService write,
            ICommonService commonService, LinkGenerator linkGenerator, IConfiguration configuration, IEmailService emailService, SignInManager<AppUser> signInManager, SearchServiceDBContext dbContext,
            RoleManager<IdentityRole> roleManager, IUserService userService, IPermissionService permService, IStorageService<StoredFile> fileStorage)
        {
            _db = db;
            _tokenService = tokenService;
            _linkGenerator = linkGenerator;
            _commonService = commonService;
            _configuration = configuration;
            _emailSender = emailService;
            _dbContext = dbContext;
            _userManager = userManager;
            _context = context;
            _dbRead = read;
            _mapper = mapper;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _write = write;
            _userService = userService;
            _permService = permService;
            _fileStorage = fileStorage;
        }

        private AppUser _appUser;

        public AppUser User
        {
            get
            {
                if (_appUser == null)
                {
                    GetUser();
                }
                return _appUser;
            }
        }

        private List<string> _roles;

        public List<string> Roles
        {
            get
            {
                if (_roles == null)
                {
                    _roles = UserManager.GetRolesAsync(User).GetAwaiter().GetResult().ToList();
                }
                return _roles;
            }
        }

        private ClientDTO _clientDTO;

        public ClientDTO ClientDto
        {
            get
            {
                if (_clientDTO == null)
                {
                    _clientDTO = GetClientFromToken();
                }
                return _clientDTO;
            }
        }

        private Client _client;

        public Client Client
        {
            get
            {
                if (_client == null)
                {
                    GetClinet();
                }
                return _client;
            }
        }

        private string _theme;
        public string Theme
        {
            get
            {
                if (string.IsNullOrEmpty(_theme))
                {
                    GetCurrentTheme();
                }
                return _theme;
            }
        }

        private List<DocType> _doctypes;

        public List<DocType> DocTypes
        {
            get
            {
                if (_doctypes == null)
                {
                    _doctypes = _dbContext.DocTypes.AsNoTracking().ToList();
                }
                return _doctypes;
            }
        }

        private UserClient _userClient;

        public UserClient UserClient
        {
            get
            {
                if (_userClient == null)
                {
                    _userClient = DataBase.SingleAsync<UserClient, UserClient>(c => c.UserId.Equals(User.Id)).GetAwaiter().GetResult();
                }
                return _userClient;
            }
        }

        private Tariffs _tariff;

        public Tariffs Tariff
        {
            get
            {
                if (_tariff == null)
                {
                    _tariff = Client.TariffId == null ? null : DbRead.GetTarif((int)Client.TariffId).GetAwaiter().GetResult();
                }
                return _tariff;
            }
        }

        //TODO - переосмыслить есть ли смысл получения пользователя
        private void GetClinet()
        {
            _client = DbRead.GetClient(User.Id).GetAwaiter().GetResult();
        }

        //TODO - переосмыслить
        private void GetUser()
        {
            AppUser user = UserManager.GetUserAsync(_context.HttpContext.User).GetAwaiter().GetResult();
            _appUser = user;
        }

        private void GetCurrentTheme()
        {
            _theme = Context.HttpContext.Request.Cookies["currentTheme"]?.ToString();
        }

        private ClientDTO GetClientFromToken()
        {
            var authHeader = Context.HttpContext.Request.Headers["Authorization"][0];
            var token = string.Empty;
            if (authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length);
            }
            _client = DbContext.Clients.AsNoTracking().Where(m => m.Token.Equals(token)).FirstOrDefault(null);
            //TOTO проверить и может убрать проверку
            if (_client == null) return null;
            return Mapper.Map<Client, ClientDTO>(_client);
        }
    }
}
