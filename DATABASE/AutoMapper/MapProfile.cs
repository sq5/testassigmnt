// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text;
using AutoMapper;
using ARCHIVE.COMMON.DTOModels;
using ARCHIVE.COMMON.DTOModels.Admin;
using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using Newtonsoft.Json;
using DATABASE.DTOModels.UI;

namespace ARCHIVE.COMMON.AutoMapper
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<ClientsTasks, ClientsTasksDTO>().ReverseMap();
            CreateMap<Client, ClientDTO>().ReverseMap();
            CreateMap<ApiLog, ApiLogDTO>().ReverseMap();
            CreateMap<EDISettings, EDISettingsDTO>().ReverseMap();
            CreateMap<BackgroundServiceLog, BackgroundServiceLogDTO>().ReverseMap();
            CreateMap<UsersTasks, UsersTasksDTO>().ReverseMap();
            CreateMap<UsersEvents, UsersEventsDTO>().ReverseMap();
            CreateMap<ExtConnection, ExtConnectionDTO>()
                .ReverseMap()
                .ForMember(d => d.Client, a => a.Ignore());

            CreateMap<DocFile, DocFileDTO>().ReverseMap();

            CreateMap<SignaturesAndEDIEvents, SignaturesAndEDIEventsDTO>()
                .ForMember(d => d.SignatureBase64, a => a.MapFrom(c => System.Convert.ToBase64String(c.SignatureBin)))
                .ReverseMap()
                .ForMember(d => d.SignatureBin, a => a.MapFrom(c => System.Convert.FromBase64String(c.SignatureBase64)));

            CreateMap<DocFile, BinariesDTO>()
                .ForMember(d => d.FileBase64, a => a.Ignore())
                .ReverseMap()
                .ForMember(d => d.FileBin, a => a.Ignore());

            CreateMap<Billing, BillingDTO>()
                .ForMember(d => d.Document, a => a.MapFrom(c => System.Convert.ToBase64String(c.Document)))
                .ForMember(d => d.Client, a => a.MapFrom(c => c.Client.Name))
                .ReverseMap()
                .ForMember(d => d.Document, a => a.MapFrom(c => System.Convert.FromBase64String(c.Document)))
                .ForMember(d => d.Client, a => a.Ignore());

            CreateMap<AdditionalFieldsMapping, AdditionalFieldsMappingDTO>()
                .ForMember(d => d.Client, a => a.MapFrom(c => c.Client.Name))
                .ReverseMap()
                .ForMember(d => d.Client, a => a.Ignore());

            CreateMap<Contract, ContractDTO>()
                .ForMember(d => d.Client, a => a.MapFrom(c => c.Client.Name))
                .ForMember(d => d.DocType, a => a.MapFrom(c => c.DocType.Name))
                .ReverseMap()
                .ForMember(d => d.OrganizationId, a => a.MapFrom(c => c.Organization.Id))
                .ForMember(d => d.ProjectId, a => a.MapFrom(c => c.Project.Id))
                .ForMember(d => d.DocKindId, a => a.MapFrom(c => c.DocKind.Id))
                .ForMember(d => d.Organization, a => a.Ignore())
                .ForMember(d => d.Project, a => a.Ignore())
                .ForMember(d => d.Client, a => a.Ignore())
                .ForMember(d => d.DocType, a => a.Ignore())
                .ForMember(d => d.DocKind, a => a.Ignore())
                .ForMember(d => d.Contractor, a => a.Ignore());

            CreateMap<ContractExtended, ExtendedContractDTO>()
                .ForMember(d => d.Client, a => a.MapFrom(c => c.Client.Name))
                .ForMember(d => d.DocType, a => a.MapFrom(c => c.DocType.Name))
                .ReverseMap()
                .ForMember(d => d.OrganizationId, a => a.MapFrom(c => c.Organization.Id))
                .ForMember(d => d.ProjectId, a => a.MapFrom(c => c.Project.Id))
                .ForMember(d => d.DocKindId, a => a.MapFrom(c => c.DocKind.Id))
                .ForMember(d => d.Organization, a => a.Ignore())
                .ForMember(d => d.Project, a => a.Ignore())
                .ForMember(d => d.Client, a => a.Ignore())
                .ForMember(d => d.DocType, a => a.Ignore())
                .ForMember(d => d.DocKind, a => a.Ignore())
                .ForMember(d => d.Contractor, a => a.Ignore());

            CreateMap<Organization, OrganizationDTO>()
                .ForMember(d => d.Client, a => a.MapFrom(c => c.Client.Name))
                .ReverseMap()
                .ForMember(d => d.Client, a => a.Ignore());

            CreateMap<NonFormDocs, NonFormDocsDTO>()
                .ForMember(d => d.Client, a => a.MapFrom(c => c.Client.Name))
                .ForMember(d => d.DocType, a => a.MapFrom(c => c.DocType.Name))
                .ReverseMap()
                .ForMember(d => d.Organization, a => a.Ignore())
                .ForMember(d => d.Client, a => a.Ignore())
                .ForMember(d => d.Contractor, a => a.Ignore())
                .ForMember(d => d.DocType, a => a.Ignore());

            CreateMap<Contractor, ContractorDTO>()
                .ForMember(d => d.Organization, a => a.MapFrom(c => c.Organization.Name))
                .ReverseMap()
                .ForMember(d => d.Organization, a => a.Ignore());

            CreateMap<Metadata, MetadataDTO>()
                .ForMember(d => d.Client, a => a.MapFrom(c => c.Client.Name))
                .ForMember(d => d.DocType, a => a.MapFrom(c => c.DocType.Name))
                .ReverseMap()
                .ForMember(d => d.ContractorId, a => a.MapFrom(c => c.Contractor.Id))
                .ForMember(d => d.OrganizationId, a => a.MapFrom(c => c.Organization.Id))
                .ForMember(d => d.ProjectId, a => a.MapFrom(c => c.Project.Id))
                .ForMember(d => d.ContractId, a => a.MapFrom(c => c.Contract.Id))
                .ForMember(d => d.DocKindId, a => a.MapFrom(c => c.DocKind.Id))
                .ForMember(d => d.Client, a => a.Ignore())
                .ForMember(d => d.Project, a => a.Ignore())
                .ForMember(d => d.Organization, a => a.Ignore())
                .ForMember(d => d.Contract, a => a.Ignore())
                .ForMember(d => d.Contractor, a => a.Ignore())
                .ForMember(d => d.DocKind, a => a.Ignore())
                .ForMember(d => d.DocType, a => a.Ignore());

            CreateMap<MetadataExtended, ExtendedMetadataDTO>()
                .ForMember(d => d.Client, a => a.MapFrom(c => c.Client.Name))
                .ForMember(d => d.DocType, a => a.MapFrom(c => c.DocType.Name))
                .ReverseMap()
                .ForMember(d => d.ContractorId, a => a.MapFrom(c => c.Contractor.Id))
                .ForMember(d => d.OrganizationId, a => a.MapFrom(c => c.Organization.Id))
                .ForMember(d => d.ProjectId, a => a.MapFrom(c => c.Project.Id))
                .ForMember(d => d.ContractId, a => a.MapFrom(c => c.Contract.Id))
                .ForMember(d => d.DocKindId, a => a.MapFrom(c => c.DocKind.Id))
                .ForMember(d => d.Client, a => a.Ignore())
                .ForMember(d => d.Project, a => a.Ignore())
                .ForMember(d => d.Organization, a => a.Ignore())
                .ForMember(d => d.Contract, a => a.Ignore())
                .ForMember(d => d.Contractor, a => a.Ignore())
                .ForMember(d => d.DocKind, a => a.Ignore())
                .ForMember(d => d.DocType, a => a.Ignore());

            CreateMap<Metadata, DocumentMobileDTO>()
                .ForMember(d => d.DocType, a => a.MapFrom(c => c.DocType.Name))
                .ForMember(d => d.Organization, a => a.MapFrom(c => c.Organization.Name))
                .ForMember(d => d.DocKind, a => a.MapFrom(c => c.DocKind.Name))
                .ForMember(d => d.Project, a => a.MapFrom(c => c.Project.Name))
                .ForMember(d => d.Contract, a => a.MapFrom(c => c.Contract.Name))
                .ForMember(d => d.Contractor, a => a.MapFrom(c => c.Contractor.Name))
                .ReverseMap()
                .ForMember(d => d.Client, a => a.Ignore())
                .ForMember(d => d.Project, a => a.Ignore())
                .ForMember(d => d.Organization, a => a.Ignore())
                .ForMember(d => d.Contract, a => a.Ignore())
                .ForMember(d => d.Contractor, a => a.Ignore())
                .ForMember(d => d.DocKind, a => a.Ignore())
                .ForMember(d => d.DocType, a => a.Ignore());

            CreateMap<Contract, DocumentMobileDTO>()
                .ForMember(d => d.DocType, a => a.MapFrom(c => c.DocType.Name))
                .ForMember(d => d.Organization, a => a.MapFrom(c => c.Organization.Name))
                .ForMember(d => d.DocKind, a => a.MapFrom(c => c.DocKind.Name))
                .ForMember(d => d.Project, a => a.MapFrom(c => c.Project.Name))
                .ForMember(d => d.Contractor, a => a.MapFrom(c => c.Contractor.Name))
                .ReverseMap()
                .ForMember(d => d.Client, a => a.Ignore())
                .ForMember(d => d.Project, a => a.Ignore())
                .ForMember(d => d.Organization, a => a.Ignore())
                .ForMember(d => d.Contractor, a => a.Ignore())
                .ForMember(d => d.DocKind, a => a.Ignore())
                .ForMember(d => d.DocType, a => a.Ignore());

            CreateMap<WFTemplates, WFTemplateDTO>()
                .ForMember(d => d.WF, a => a.MapFrom(c => JsonConvert.DeserializeObject<List<UsersTasksDTO>>(Encoding.UTF8.GetString(c.WF))))
                .ReverseMap()
                .ForMember(d => d.WF, a => a.MapFrom(c => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(c.WF))));

            CreateMap<Project, ProjectDTO>()
                .ForMember(d => d.ClientName, a => a.MapFrom(c => c.Client.Name))
                .ReverseMap()
                .ForMember(d => d.Client, a => a.Ignore());
            CreateMap<ClientsTemplates, ClientsTemplatesWithFileDTO>()
                .ForMember(d => d.ClientName, a => a.MapFrom(c => c.Client.Name))
                .ReverseMap()
                .ForMember(d => d.Client, a => a.Ignore());
        }
    }
}
