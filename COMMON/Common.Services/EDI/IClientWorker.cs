namespace CloudArchive.Services.EDI
{
    public interface IClientWorker
    {
        IEDIJobSettings Settings { get; set; }

        bool Completed { get; set; }

        void ProcessBatch();

        void FindNextBatch();

        void ProcessDocument();
    }
}
