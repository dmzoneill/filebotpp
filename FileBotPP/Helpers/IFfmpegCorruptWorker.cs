namespace FileBotPP.Helpers
{
    public interface IFfmpegCorruptWorker
    {
        void stop_worker();
        void start_scan();
        void Dispose();
    }
}