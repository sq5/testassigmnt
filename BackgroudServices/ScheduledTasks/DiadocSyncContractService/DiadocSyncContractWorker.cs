// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ARCHIVE.COMMON.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Diadoc.Api.Proto.Documents;
using CloudArchive.Services.EDI;
using CloudArchive.Services.EDI.EnsolDiadoc;
using System.Threading;
using System.Globalization;
using Diadoc.Api.Proto.Events;

namespace CloudArchive.ScheduledTasks
{
    public class DiadocSyncContractWorker : IClientWorker, IClientContractWorker
    {
        private DiadocJobSettings settings { get; set; }
        public IEDIJobSettings Settings { get { return settings; } set { settings = value as DiadocJobSettings; } }
        public bool Completed { get; set; } = false;
        public List<Contract> Documents { get; set; } = new List<Contract>();
        public Contract CurrentDocument { get; set; }

        public DiadocSyncContractWorker(DiadocJobSettings sett)
        {
            Settings = sett;
        }

        public void ProcessBatch()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
            if (DiadocCommon.Connect(settings))
            {
                FindNextBatch();
                foreach (Contract document in Documents)
                {
                    CurrentDocument = document;
                    ProcessDocument();
                }
                if (Documents.Count < settings.ClientSettings.TakeCount)
                {
                    Completed = true;
                }
                settings.ediSettings.Token = "";
            }
            else
            {
                Completed = true;
            }
        }

        public void FindNextBatch()
        {
            Documents = settings.generalSettings.DbContext.Contracts
                .Where(x => x.ClientId == settings.EdiSettings.ConnectionInfo.ClientID &&
                (x.EDIProvider == "Diadoc" || x.EDIProvider == "Диадок") &&
                x.EDIProcessed != true &&
                x.Organization.INN == settings.EdiSettings.ConnectionInfo.OrganizationINN &&
                x.Organization.KPP == settings.EdiSettings.ConnectionInfo.OrganizationKPP &&
                !string.IsNullOrEmpty(x.EDIId) &&
                x.Deleted == false)
                .Skip(settings.ClientSettings.Skip).Take(settings.ClientSettings.TakeCount).ToList();
        }

        public void ProcessDocument()
        {
            try
            {
                string mixedID = CurrentDocument.EDIId;
                string MessageID = mixedID.Substring(0, mixedID.Length / 2);
                string EntityID = mixedID.Substring(mixedID.Length / 2);
                string BoxID = settings.EdiSettings.ConnectionInfo.EDIUserID;
                Document doc = settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, MessageID, EntityID);
                string status;
                if (DiadocCommon.CheckIfStatusIsFinal(doc, CurrentDocument.EDIIsIncoming, out status))
                {
                    foreach (DiadocFileInfo file in DiadocCommon.DownloadFiles(MessageID, EntityID, BoxID, doc, settings, false))
                    {
                        DiadocCommon.AddorUpdateContractDocument(settings, file, CurrentDocument);
                    }
                    var msg = settings.ediSettings.Connection.GetMessage(settings.ediSettings.Token, BoxID, MessageID);
                    string Comment = "";
                    foreach (var en in msg.Entities)
                    {
                        if (en.AttachmentType == AttachmentType.XmlSignatureRejection || en.AttachmentType == AttachmentType.SignatureRequestRejection)
                        {
                            Comment = DiadocCommon.FindComment(settings, en, msg);
                        }
                    }
                    DiadocCommon.AddSignaturesAndEDIEvents(settings, status, DateTime.Now, Comment, CurrentDocument.Id, 0);
                    CurrentDocument.EDIProcessed = true;
                    settings.generalSettings.LogService.AddInfo("Обновлен договор " + CurrentDocument.Name + " Ид: " + CurrentDocument.Id, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
                }
                else
                    settings.ClientSettings.Skip++;
                CurrentDocument.EDIState = status;
                settings.generalSettings.DbContext.SaveChanges();

            }
            catch (Exception e)
            {
                settings.generalSettings.LogService.AddError("Произошла ошибка во время обновления статуса договора " + CurrentDocument.Name + " Ид: " + CurrentDocument.Id + " : " + e.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
                settings.ClientSettings.Skip++;
            }
        }
    }
}
