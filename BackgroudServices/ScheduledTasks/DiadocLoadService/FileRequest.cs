// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using CloudArchive.Services.EDI.EnsolDiadoc;
using Diadoc.Api.Proto.Documents;
using Diadoc.Api.Proto.Events;
using System;
using System.Linq;

namespace BackgroudServices.ScheduledTasks.DiadocLoadService
{
    public static class FileRequest
    {
        public static bool CheckFiles(DiadocJobSettings settings, BoxEvent boxEvent)
        {
            string MessageId = boxEvent.MessageId;
            string BoxID = settings.ediSettings.ConnectionInfo.EDIUserID.Replace("-", "");
            try
            {
                Message msg = settings.ediSettings.Connection.GetMessage(settings.ediSettings.Token, BoxID, MessageId);
                if (msg == null || msg.IsDeleted || msg.IsDraft || msg.MessageType == MessageType.Template)
                    return false;
                string DocID = "";
                for (int i = 0; i < boxEvent.EntitiesList.Count; i++)
                {
                    try
                    {
                        Entity en = boxEvent.Entities[i] as Entity;
                        DocID = GetDoc(msg, en);
                        if(!msg.Entities.Where(x => x.EntityId == DocID).Any())
                            continue;
                        var Doc = settings.ediSettings.Connection.GetDocument(settings.ediSettings.Token, BoxID, msg.MessageId, DocID);
                        if (Doc.IsDeleted)
                            continue;
                        settings.ediSettings.Connection.GenerateDocumentZip(settings.ediSettings.Token, BoxID, MessageId, DocID, true);
                        settings.ediSettings.Connection.GeneratePrintForm(settings.ediSettings.Token, BoxID, MessageId, DocID);
                    }
                    catch (Exception e)
                    {
                        settings.generalSettings.LogService.AddError("Произошла ошибка при запросе печатной формы по документу " + MessageId + "_" + DocID + " : " + e.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                settings.generalSettings.LogService.AddError("Произошла ошибка при запросе печатной формы по событию " + boxEvent.EventId + " : " + ex.Message, settings.generalSettings.ServiceName, settings.ediSettings.ConnectionInfo.ClientID);
                return false;
            }
        }

        private static string GetDoc(Message msg, Entity en)
        {
            string DocID = DiadocCommon.GetTopParent(msg, en.ParentEntityId);
            Entity docEntity = msg.Entities.Where(x => x.EntityId == DocID).FirstOrDefault();
            if (docEntity == null)
            {
                DocID = DiadocCommon.GetTopParent(msg, en.EntityId);
                docEntity = msg.Entities.Where(x => x.EntityId == DocID).FirstOrDefault();
            }
            return DocID;
        }
    }
}
