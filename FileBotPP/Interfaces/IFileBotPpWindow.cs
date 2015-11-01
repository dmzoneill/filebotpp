using System.Windows.Threading;
using MahApps.Metro.Controls.Dialogs;

namespace FileBotPP.Interfaces
{
    public interface IFileBotPpWindow
    {
        Dispatcher Dispatcher { get; }
        ProgressDialogController FolderScannerToast { get; set; }
        void set_status_text( string text );
        void set_eztv_progress( string text );
        void set_tvdb_progress( string text );
        void set_season_count( string text );
        void set_series_count( string text );
        void set_episode_count( string text );
        void set_ready( bool ready );
    }
}