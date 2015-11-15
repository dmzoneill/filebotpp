namespace FileBotPP.Helpers
{
    public interface IFfmpegConvertWorker
    {
        void Dispose();
        void stop_worker();
        void start_convert();
    }
}