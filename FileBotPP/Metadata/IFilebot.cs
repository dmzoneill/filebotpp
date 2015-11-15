using FileBotPP.Tree;

namespace FileBotPP.Metadata
{
    public interface IFilebot
    {
        void check_series( IDirectoryItem directory );
        void check_series_all();
        void stop_worker();
    }
}