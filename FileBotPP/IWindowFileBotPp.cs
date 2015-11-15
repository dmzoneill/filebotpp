using System.Windows.Threading;

namespace FileBotPP
{
    public interface IWindowFileBotPp
    {
        Dispatcher Dispatcher { get; }
        void set_status_text( string text );
        void set_eztv_progress( string text );
        void set_tvdb_progress( string text );
        void set_kat_progress( string text );
        void set_season_count( string text );
        void set_series_count( string text );
        void set_episode_count( string text );
        void set_ready( bool ready );
    }
}