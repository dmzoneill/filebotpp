using System.Collections.Generic;

namespace FileBotPP.Metadata
{
    public interface IEztv
    {
        void downloads_series_data();
        void free_workers();
        void stop_worker();
    }
}