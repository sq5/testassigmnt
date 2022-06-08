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
using DATABASE.Migrations;
using DATABASE.Services;
using ARCHIVE.COMMON.Servises;
using AutoMapper;
using CloudArchive.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using ARCHIVE.COMMON.Extensions.DateTimeModelBinder;
using CloudArchive.Services.ClientService;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using CloudArchive.Services.PermissionService;
using COMMON.Common.Services.ContextService;
using COMMON.Common.Services.StorageService;
using COMMON.Models;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using System.Collections.Generic;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<SearchServiceDBContext>(options =>
            {
                //options.EnableSensitiveDataLogging(true); //!!! Только для разработки!
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
            services.AddResponseCompression(options => options.EnableForHttps = true);
            services.AddScoped<IStorageService<StoredFile>, AzureStorageService<StoredFile>>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IPermissionService, PermissionService>();
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IContextService, ContextService>();

            services.AddControllersWithViews();
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                })
                .AddRazorRuntimeCompilation();

            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddAutoMapper(typeof(Startup), typeof(Client), typeof(Organization), typeof(Metadata));
        }

        public void Configure(IApplicationBuilder app,
                              IWebHostEnvironment env,
                              SearchServiceDBContext db,
                              UserManager<AppUser> userManager,
                              RoleManager<IdentityRole> roleManager)
        {
            app.UseResponseCompression();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            //Здесь я хочу управлять своими статическими файлами сценариев

            //app.UseAuthentication();
            app.UseSession();
            //app.UseCookiePolicy();
            
            app.UseRouting();
            //app.UseAuthentication();
           // app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
