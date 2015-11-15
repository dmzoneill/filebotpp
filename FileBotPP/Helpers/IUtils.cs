using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace FileBotPP.Helpers
{
    public interface IUtils
    {
        string Fetch( string url );
        string FetchDeCompressed( string url );
        bool download_file( string url, string filename );
        string read_file_from_zip( string zipfile, string internalfile );
        bool write_file( string filename, string contents );
        string get_process_output( string processname, string arguments, int waittime = 3000 );
        int run_process_foreground( string processname, string arguments );
        int run_process_background( string processname, string arguments );
        bool check_for_internet_connection();
        bool check_for_eztv_connection();
        bool check_for_tvdb_connection();
        bool check_for_website( string url );
        void download_torrent( string magneturl );
        void open_file( string file );
        T get_visual_parent<T>( Visual referencedVisual ) where T : Visual;
        T get_visual_child<T>( Visual referencedVisual ) where T : Visual;
        List< Visual > AllChildren( DependencyObject parent );
    }
}