using System.Collections.Generic;

namespace FileBotPP.Metadata
{
    internal interface IEztvWorker
    {
        void Run();
        bool is_working();
        string get_html();
        string get_series_name();
        bool is_processed();
        List< ITorrent > get_torrents();
        bool is_cached();
    }
}