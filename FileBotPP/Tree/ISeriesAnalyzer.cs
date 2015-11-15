namespace FileBotPP.Tree
{
    public interface ISeriesAnalyzer
    {
        void analyze_all_series_folders();
        void analyze_series_folder( IDirectoryItem folder );
    }
}