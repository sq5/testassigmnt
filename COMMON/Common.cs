// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using ARCHIVE.COMMON.DTOModels.UI;
using ARCHIVE.COMMON.Entities;
using ARCHIVE.COMMON.DTOModels.Admin;
using DATABASE.Context;
using Microsoft.EntityFrameworkCore;

namespace Ensol.CommonUtils
{
    public static class Common
    {
        public static ExtendedMetadataDTO GetMetadataByID(long metaID, SearchServiceDBContext dbContext, int ClientID = -1)
        {
            try
            {
                IQueryable<MetadataExtended> metaDBcoll = null;
                if (ClientID == -1)
                    metaDBcoll = dbContext.MetadatasExtended.Where(x => x.Id == metaID)
                .Include(m => m.Contract).Include(m => m.DocType).Include(m => m.DocKind).Include(m => m.Contractor).Include(m => m.Organization).Include(m => m.Client).Include(m => m.Project);
                else
                    metaDBcoll = dbContext.MetadatasExtended.Where(x => x.Id == metaID && x.ClientId == ClientID)
                .Include(m => m.Contract).Include(m => m.DocType).Include(m => m.DocKind).Include(m => m.Contractor).Include(m => m.Organization).Include(m => m.Client).Include(m => m.Project);
                if (metaDBcoll.Any())
                {
                    MetadataExtended metaDB = metaDBcoll.FirstOrDefault();
                    OrganizationDTO org = null;
                    if (metaDB.OrganizationId.HasValue)
                    {
                        org = new OrganizationDTO();
                        org.Name = metaDB.Organization.Name;
                        org.Id = metaDB.OrganizationId.Value;
                    }
                    ContractDTO contract = null;
                    if (metaDB.ContractId.HasValue)
                    {
                        contract = new ContractDTO();
                        contract.Name = metaDB.Contract.Name;
                        contract.Id = metaDB.ContractId.Value;
                    }
                    ContractorDTO contractor = null;
                    if (metaDB.ContractorId.HasValue)
                    {
                        contractor = new ContractorDTO();
                        contractor.Name = metaDB.Contractor.Name;
                        contractor.Id = metaDB.ContractorId.Value;
                    }
                    ProjectDTO project = null;
                    if (metaDB.ProjectId.HasValue)
                    {
                        project = new ProjectDTO();
                        project.Name = metaDB.Project.Name;
                        project.Id = metaDB.ProjectId.Value;
                        project.ProjectLeader = metaDB.Project.ProjectLeader;
                        project.State = metaDB.Project.State;
                    }
                    ExtendedMetadataDTO meta = new ExtendedMetadataDTO()
                    {
                        Id = metaDB.Id,
                        Modified = metaDB.Modified,
                        ModifiedBy = metaDB.ModifiedBy,
                        Organization = org,
                        Source = metaDB.Source,
                        VAT = metaDB.VAT,
                        Amount = metaDB.Amount,
                        AmountWOVAT = metaDB.AmountWOVAT,
                        AmountToPay = metaDB.AmountToPay,
                        ClientId = metaDB.ClientId ?? -1,
                        Created = metaDB.Created,
                        CreatedBy = metaDB.CreatedBy,
                        Currency = metaDB.Currency,
                        DocDate = metaDB.DocDate,
                        Comment = metaDB.Comment,
                        PeriodFrom = metaDB.PeriodFrom,
                        PeriodTo = metaDB.PeriodTo,
                        Reciever = metaDB.Reciever,
                        Address = metaDB.Address,
                        Delivery = metaDB.Delivery,
                        Contact = metaDB.Contact,
                        DocNumTaxInvoice = metaDB.DocNumTaxInvoice,
                        DocDateTaxInvoice = metaDB.DocDateTaxInvoice == DateTime.MinValue ? null : metaDB.DocDateTaxInvoice,
                        DocNumInvoice = metaDB.DocNumInvoice,
                        DocDateInvoice = metaDB.DocDateInvoice == DateTime.MinValue ? null : metaDB.DocDateInvoice,
                        DocNumber = metaDB.DocNumber,
                        DocTypeId = metaDB.DocTypeId,
                        DocType = metaDB.DocType == null ? string.Empty : metaDB.DocType.Name,
                        DocKind = metaDB.DocKind,
                        Contract = contract,
                        Signed = metaDB.Signed,
                        Paid = metaDB.Paid,
                        Contractor = contractor,
                        FileExists = false,
                        EDIProvider = metaDB.EDIProvider,
                        EDIId = metaDB.EDIId,
                        EDIProcessed = metaDB.EDIProcessed,
                        EDIState = metaDB.EDIState,
                        Ext_ID = metaDB.Ext_ID,
                        State = metaDB.State,
                        EDILocalSigned = metaDB.EDILocalSigned,
                        CardData = metaDB.CardData,
                        UploadedTo1C = metaDB.UploadedTo1C.HasValue ? metaDB.UploadedTo1C.Value : false,
                        EDIIsIncoming = metaDB.EDIIsIncoming,
                        EdiNeedExport = metaDB.EdiNeedExport,
                        PackageID = metaDB.PackageID,
                        NotifyUser = metaDB.NotifyUser,
                        PaymentDate = metaDB.PaymentDate,
                        PaymentNumber = metaDB.PaymentNumber,
                        Deleted = metaDB.Deleted,
                        DeleteDate = metaDB.DeleteDate,
                        TablePart = metaDB.TablePart,
                        Client = metaDB.Client == null ? "" : metaDB.Client.Name,
                        Project = metaDB.Project,
                        String1 = metaDB.String1,
                        String2 = metaDB.String2,
                        String3 = metaDB.String3,
                        String4 = metaDB.String4,
                        String5 = metaDB.String5,
                        String6 = metaDB.String6,
                        String7 = metaDB.String7,
                        String8 = metaDB.String8,
                        Int1 = metaDB.Int1,
                        Int2 = metaDB.Int2,
                        Int3 = metaDB.Int3,
                        Int4 = metaDB.Int4,
                        Datetime1 = metaDB.Datetime1,
                        Datetime2 = metaDB.Datetime2,
                        Datetime3 = metaDB.Datetime3,
                        Datetime4 = metaDB.Datetime4,
                        Bool1 = metaDB.Bool1,
                        Bool2 = metaDB.Bool2,
                        Bool3 = metaDB.Bool3,
                        Bool4 = metaDB.Bool4
                    };
                    return meta;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static ExtendedContractDTO GetContractByID(int ContractID, SearchServiceDBContext dbContext, int ClientID = -1)
        {
            try
            {
                IQueryable<ContractExtended> ctDBcoll = null;
                if (ClientID == -1)
                    ctDBcoll = dbContext.ContractsExtended.Where(x => x.Id == ContractID)
                .Include(m => m.DocType).Include(m => m.DocKind).Include(m => m.Contractor).Include(m => m.Organization).Include(m => m.Client).Include(m => m.Project);
                else
                    ctDBcoll = dbContext.ContractsExtended.Where(x => x.Id == ContractID && x.ClientId == ClientID)
                .Include(m => m.DocType).Include(m => m.DocKind).Include(m => m.Contractor).Include(m => m.Organization).Include(m => m.Client).Include(m => m.Project);
                if (ctDBcoll.Any())
                {
                    ContractExtended ctDB = ctDBcoll.FirstOrDefault();

                    ContractorDTO contractor = new ContractorDTO();
                    contractor.Name = ctDB.Contractor.Name;
                    contractor.Id = ctDB.ContractorId;

                    OrganizationDTO org = new OrganizationDTO();
                    org.Name = ctDB.Organization.Name;
                    org.Id = ctDB.OrganizationId;

                    ProjectDTO project = null;
                    if (ctDB.ProjectId.HasValue)
                    {
                        project = new ProjectDTO();
                        project.Name = ctDB.Project.Name;
                        project.Id = ctDB.ProjectId.Value;
                        project.ProjectLeader = ctDB.Project.ProjectLeader;
                        project.State = ctDB.Project.State;
                    }
                    ExtendedContractDTO ct = new ExtendedContractDTO()
                    {
                        Id = ctDB.Id,
                        Ext_ID = ctDB.Ext_ID,
                        DocNumber = ctDB.DocNumber,
                        DocDate = ctDB.DocDate == DateTime.MinValue ? null : ctDB.DocDate,
                        ContractorName = ctDB.Contractor.Name,
                        OrganizationName = ctDB.Organization.Name,
                        Modified = ctDB.Modified,
                        ModifiedBy = ctDB.ModifiedBy,
                        Client = ctDB.Client?.Name,
                        Created = ctDB.Created,
                        Contractor = contractor,
                        Organization = org,
                        Type = ctDB.Type,
                        Name = ctDB.Name,
                        Amount = ctDB.Amount,
                        AmountWOVAT = ctDB.AmountWOVAT,
                        VAT = ctDB.VAT,
                        Currency = ctDB.Currency,
                        Comment = ctDB.Comment,
                        Subject = ctDB.Subject,
                        DocKind = ctDB.DocKind,
                        DocTypeId = ctDB.DocTypeId,
                        DocType = ctDB.DocType?.Name,
                        ValidityPeriod = ctDB.ValidityPeriod == DateTime.MinValue ? null : ctDB.ValidityPeriod,
                        FileExists = false,
                        EDIProvider = ctDB.EDIProvider,
                        EDIId = ctDB.EDIId,
                        EDIState = ctDB.EDIState,
                        EDIProcessed = ctDB.EDIProcessed,
                        State = ctDB.State,
                        ClientId = ctDB.ClientId.HasValue ? ctDB.ClientId.Value : -1,
                        CreatedBy = ctDB.CreatedBy,
                        Deleted = ctDB.Deleted,
                        DeleteDate = ctDB.DeleteDate,
                        EDIIsIncoming = ctDB.EDIIsIncoming,
                        EDILocalSigned = ctDB.EDILocalSigned,
                        EdiNeedExport = ctDB.EdiNeedExport,
                        OrganizationId = ctDB.OrganizationId,
                        Project = ctDB.Project,
                        String1 = ctDB.String1,
                        String2 = ctDB.String2,
                        String3 = ctDB.String3,
                        String4 = ctDB.String4,
                        String5 = ctDB.String5,
                        String6 = ctDB.String6,
                        String7 = ctDB.String7,
                        String8 = ctDB.String8,
                        Int1 = ctDB.Int1,
                        Int2 = ctDB.Int2,
                        Int3 = ctDB.Int3,
                        Int4 = ctDB.Int4,
                        Datetime1 = ctDB.Datetime1,
                        Datetime2 = ctDB.Datetime2,
                        Datetime3 = ctDB.Datetime3,
                        Datetime4 = ctDB.Datetime4,
                        Bool1 = ctDB.Bool1,
                        Bool2 = ctDB.Bool2,
                        Bool3 = ctDB.Bool3,
                        Bool4 = ctDB.Bool4
                    };


                    return ct;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static NonFormDocsDTO GetNonFormDocByID(int NonFormDoID, SearchServiceDBContext dbContext, int ClientID = -1)
        {
            try
            {
                IQueryable<NonFormDocs> nfDBcoll = null;
                if (ClientID == -1)
                    nfDBcoll = dbContext.NonFormDocs.Where(x => x.Id == NonFormDoID)
                .Include(m => m.Organization).Include(m => m.Client);
                else
                    nfDBcoll = dbContext.NonFormDocs.Where(x => x.Id == NonFormDoID && x.ClientId == ClientID)
                     .Include(m => m.Organization).Include(m => m.Client);
                if (nfDBcoll.Any())
                {
                    NonFormDocs nfDB = nfDBcoll.FirstOrDefault();

                    OrganizationDTO org = null;
                    if (nfDB.OrganizationId.HasValue)
                    {
                        org = new OrganizationDTO();
                        org.Name = nfDB.Organization?.Name;
                        org.Id = nfDB.OrganizationId.Value;
                    }

                    NonFormDocsDTO ct = new NonFormDocsDTO()
                    {
                        Id = nfDB.Id,
                        OrganizationName = nfDB.Organization?.Name,
                        Modified = nfDB.Modified,
                        Client = nfDB.Client?.Name,
                        Created = nfDB.Created,
                        Organization = org,
                        Comment = nfDB.Comment,
                        FileExists = false,
                        ClientId = nfDB.ClientId.HasValue ? nfDB.ClientId.Value : -1,
                        Deleted = nfDB.Deleted,
                        DeleteDate = nfDB.DeleteDate,
                        OrganizationId = nfDB.OrganizationId,
                        Declined = nfDB.Declined,
                        DeclinedBy = nfDB.DeclinedBy,
                        Sender = nfDB.Sender,
                        OCRVerified = nfDB.OCRVerified
                    };
                    return ct;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
