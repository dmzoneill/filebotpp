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
        bool is_cached();
    }
}