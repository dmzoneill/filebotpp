using System.Windows.Threading;

namespace FileBotPP.Interfaces
{
    public interface IFileBotPpWindow
    {
        void set_status_text( string text );
        void set_eztv_progress( string text );
        void set_tvdb_progress( string text );
        void set_season_count( string text );
        void set_series_count( string text );
        void set_episode_count( string text );
        Dispatcher Dispatcher { get; }
    }
}