using System;
using Diadoc.Api.Proto.Events;
using CloudArchive.Services.EDI;
using CloudArchive.Services.EDI.EnsolDiadoc;
using System.Globalization;
using System.Threading;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1;
using BackgroudServices.ScheduledTasks.DiadocLoadService;

namespace CloudArchive.ScheduledTasks
{
    public class DiadocLoadClientWorker : IClientWorker
    {
        public BoxEventList Events { get; set; }
        public BoxEvent CurrentEvent { get; set; }
        private DiadocJobSettings settings { get; set; }
        public IEDIJobSettings Settings { get { return settings; } set { settings = value as DiadocJobSettings; } }
        public DiadocDocumentLoader loader { get; set; }
        public bool Completed { get; set; } = false;
        private int loadCounter = 0;
        private readonly int TakeCount = 10;

        public DiadocLoadClientWorker(DiadocJobSettings DiadocSettings)
        {
            Settings = DiadocSettings;
            loader = new DiadocDocumentLoader(settings);
        }

        public void ProcessBatch()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
            if (DiadocCommon.Connect(settings))
            {
                FindNextBatch();
                foreach (var BoxEvent in Events.Events)
                {
                    FileRequest.CheckFiles(settings, BoxEvent);
                }
                foreach (var BoxEvent in Events.Events)
                {
                    CurrentEvent = BoxEvent;
                    ProcessDocument();
                    if (loadCounter > TakeCount)
                        break;
                }
                if (Events.Events.Count < Settings.ClientSettings.TakeCount)
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
            loadCounter = 0;
            long ticks = settings.ediSettings.ConnectionInfo.LastEventDate.HasValue ? settings.ediSettings.ConnectionInfo.LastEventDate.Value.Ticks : DateTime.Now.Ticks;

            Events = settings.ediSettings.Connection.GetNewEvents(settings.ediSettings.Token, settings.ediSettings.ConnectionInfo.EDIUserID, null, null, null, null, null, null, ticks);

            //Events = settings.ediSettings.Connection.GetNewEvents(settings.ediSettings.Token, settings.ediSettings.ConnectionInfo.EDIUserID, settings.ediSettings.ConnectionInfo.LastEvent);
        }

        public void ProcessDocument()
        {
            if (loader.Load(CurrentEvent))
            {
                loadCounter++;
            }
            settings.ediSettings.ConnectionInfo.LastEvent = CurrentEvent.EventId;
            settings.ediSettings.ConnectionInfo.LastEventDate = CurrentEvent.Timestamp;
            settings.generalSettings.DbContext.SaveChanges();
        }
    }
}
