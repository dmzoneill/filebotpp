using System.Collections.Generic;

namespace FileBotPP.Metadata
{
    public interface ITvdb
    {
        void downloads_series_data();
        void downloads_series_data( string name );
        void get_series_from_workers();
        List< ITvdbSeries > get_series();
        ITvdbSeries get_series_by_name( string name );
        void free_workers();
        void stop_worker();
    }
}