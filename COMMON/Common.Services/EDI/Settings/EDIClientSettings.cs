namespace CloudArchive.Services.EDI.Settings
{
    public class EDIClientSettings : IEDIClientSettings
    {
        private int takeCount = 10;
        private int skip = 0;
        public int TakeCount { get { return takeCount; } set { takeCount = value; } }
        public int Skip { get { return skip; } set { skip = value; } }
    }
}
