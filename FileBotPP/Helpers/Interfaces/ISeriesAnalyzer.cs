using FileBotPP.Tree.Interfaces;

namespace FileBotPP.Helpers.Interfaces
{
    public interface ISeriesAnalyzer
    {
        void fetch_tvdb_metadata();
        void fetch_eztv_metadata();
        void analyze_all_series_folders();
        void analyze_series_folder( IDirectoryItem folder );
    }
}