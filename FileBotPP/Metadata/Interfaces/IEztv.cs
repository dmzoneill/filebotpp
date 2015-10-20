using System.Collections.Generic;
using FileBotPP.Metadata.eztv.Interfaces;

namespace FileBotPP.Metadata.Interfaces
{
    public interface IEztv
    {
        void downloads_series_data();
        List< ITorrent > get_torrents();
        void get_series_from_workers();
        void free_workers();
        void stop_worker();
    }
}