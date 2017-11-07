using System.Collections.Generic;

namespace FileBotPP.Metadata
{
    internal interface IKatWorker
    {
        void Run();
        bool is_working();
        string get_series_name();
        bool is_cached();
    }
}