// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ARCHIVE.COMMON.Entities;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System;

namespace DATABASE.Context
{
    public class SearchServiceDBContext : IdentityDbContext<AppUser>
    {
        public DbSet<UserClient> AppUsers { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public DbSet<Metadata> Metadatas { get; set; }
        public DbSet<DocFile> Files { get; set; }
        public DbSet<DocType> DocTypes { get; set; }
        public DbSet<DocKind> DocKinds { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public DbSet<Contractor> Contractors { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ApiLog> ApiLogs { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public virtual DbSet<NonFormDocs> NonFormDocs { get; set; }
        public DbSet<Tariffs> Tariffs { get; set; }
        public DbSet<ExtConnection> ExtConnections { get; set; }
        public DbSet<BackgroundServiceLog> BackgroundServiceLogs { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<Versions> Versions { get; set; }
        public DbSet<ClientsTasks> ClientsTasks { get; set; }
        public DbSet<ReestrPerms> ReestrPerms { get; set; }
        public DbSet<UsersTasks> UsersTasks { get; set; }
        public DbSet<EDISettings> EDISettings { get; set; }
        public DbSet<Favorites> Favorites { get; set; }
        public DbSet<OrgPerms> OrgPerms { get; set; }
        public DbSet<ExtExchangeSetting> ExtExchangeSettings { get; set; }
        public DbSet<SignaturesAndEDIEvents> SignaturesAndEDIEvents { get; set; }
        public DbSet<EDISignPerms> EDISignPerms { get; set; }
        public DbSet<UsersSetting> UserSettings { get; set; }
        public DbSet<UsersEvents> UsersEvents { get; set; }
        public DbSet<WFTemplates> WFTemplates { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectPerms> ProjectPerms { get; set; }
        public DbSet<Substitution> Substitutions { get; set; }
        public DbSet<ClientsTemplates> ClientsTemplates { get; set; }
        public DbSet<AdditionalFieldsMapping> AdditionalFieldsMappings { get; set; }
        public DbSet<AdditionalField> AdditionalFields { get; set; }
        public DbSet<ContractExtended> ContractsExtended { get; set; }
        public DbSet<MetadataExtended> MetadatasExtended { get; set; }

        public SearchServiceDBContext(DbContextOptions<SearchServiceDBContext> options) : base(options)
        {
        }
        public SearchServiceDBContext() : base()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder().Build();
                var connectionString = "server=Localhost;port=3306;database=EnsolCloud;Uid=root;Pwd=Kostik_53";
                optionsBuilder.UseMySql(connectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            builder.Entity<UserClient>().HasKey(uc => new { uc.UserId, uc.ClientId });
            //builder.Entity<UserClient>().HasKey(uc => uc.ClientId);

            builder.Entity<ContractExtended>().ToView("ContractsExtended").HasKey(t => t.Id);
            builder.Entity<MetadataExtended>().ToView("MetadatasExtended").HasKey(t => t.Id);

            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public IQueryable<Metadata> Where(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }
    }
}
