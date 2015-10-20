namespace FileBotPP.Helpers.Interfaces
{
    public interface ISeriesAnalyzer
    {
        void fetch_tvdb_metadata();
        void fetch_eztv_metadata();
        void analyze_series_folder();
    }
}