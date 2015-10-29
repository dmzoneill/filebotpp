using System.Collections.Generic;
using FileBotPP.Metadata.tvdb.Interfaces;

namespace FileBotPP.Metadata.Interfaces
{
    public interface ITvdb
    {
        void downloads_series_data();
        void downloads_series_data(string name);
        void get_series_from_workers();
        List< ISeries > get_series();
        ISeries get_series_by_name( string name );
        void free_workers();
        void stop_worker();
    }
}