namespace FileBotPP.Metadata
{
    internal interface ITvdbWorker
    {
        void Run();
        bool is_working();
        ITvdbSeries get_series();
        string get_series_name();
        bool is_cached();
        void parse_series_data();
        string[] get_artwork_links();
    }
}