// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DATABASE.Context;
using ARCHIVE.COMMON.Entities;
using DATABASE.Services;
using ARCHIVE.COMMON.Servises;
using AutoMapper;
using CloudArchive.Services;
using ARCHIVE.COMMON.Extensions.DateTimeModelBinder;
using CloudArchive.Services.ClientService;
using BackgroudServices.Scheduling;
using CloudArchive.ScheduledTasks;
using COMMON.Common.Services.StorageService;
using COMMON.Models;
using COMMON.Common.Services.ContextService;
using CloudArchive.Services.PermissionService;

namespace BackgroudServices
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<SearchServiceDBContext>(options =>
            {
                //options.EnableSensitiveDataLogging(true); //!!! ������ ��� ����������!
                options.UseMySql(Configuration["ConnectionStringMySql"], providerOptions => providerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
            });
            services.AddDefaultIdentity<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<SearchServiceDBContext>();

            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(360);
                options.SlidingExpiration = true;
            });

            services.AddScoped<IStorageService<StoredFile>, AzureStorageService<StoredFile>>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IDBReadService, DbReadService>();
            services.AddScoped<IDBWriteService, DBWriteService>();
            services.AddScoped<IUIReadService, UIReadService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAdminService, AdminEFService>();
            services.AddScoped<IBackgroundServiceLog, TimerJobLogService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IVerification, Verification>();
            services.AddScoped<IContextService, ContextService>();
            services.AddScoped<IPermissionService, PermissionService>();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
                })
                .AddRazorRuntimeCompilation();

            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddAutoMapper(typeof(Startup), typeof(Client), typeof(Organization), typeof(Metadata));

      
            services.AddSingleton<IScheduledTask, MaintanceBackgroundService>();

            //services.AddSingleton<IScheduledTask, MigrationService>();

            services.AddScheduler((sender, args) =>
            {
                args.SetObserved();
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            //app.UseRouting();
            //app.UseAuthorization();
        }
    }
}
