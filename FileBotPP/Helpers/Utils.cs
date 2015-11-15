using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileBotPP.Helpers
{
    public class Utils : IUtils
    {
        public string Fetch( string url )
        {
            var data = "";

            try
            {
                var wrGeturl = WebRequest.Create( url );

                if ( string.Compare( Factory.Instance.Settings.ProxyServerHost, "", StringComparison.Ordinal ) != 0 )
                {
                    var wp = new WebProxy( Factory.Instance.Settings.ProxyServerHost + ":" + Factory.Instance.Settings.ProxyServerPort, true );
                    wrGeturl.Proxy = wp;
                }
                else
                {
                    wrGeturl.Proxy = null;
                }

                var objStream = wrGeturl.GetResponse().GetResponseStream();
                if ( objStream != null )
                {
                    var objReader = new StreamReader( objStream );
                    data = objReader.ReadToEnd();
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                data = "";
            }

            return data;
        }

        public string FetchDeCompressed( string url )
        {
            try
            {
                var client = new ZlibWebClient {Proxy = null};

                return client.DownloadString( url );
            }
            catch ( Exception )
            {
                return null;
            }
        }

        public bool download_file( string url, string filename )
        {
            try
            {
                var client = new WebClient();

                if ( string.Compare( Factory.Instance.Settings.ProxyServerHost, "", StringComparison.Ordinal ) != 0 )
                {
                    var wp = new WebProxy( Factory.Instance.Settings.ProxyServerHost + ":" + Factory.Instance.Settings.ProxyServerPort, true );
                    client.Proxy = wp;
                }
                else
                {
                    client.Proxy = null;
                }

                client.DownloadFile( url, filename );
                return true;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return false;
            }
        }

        public string read_file_from_zip( string zipfile, string internalfile )
        {
            try
            {
                var zipStream = new FileStream( zipfile, FileMode.Open );
                var archive = new ZipArchive( zipStream, ZipArchiveMode.Read );

                foreach ( var entry in archive.Entries )
                {
                    if ( String.Compare( entry.Name, internalfile, StringComparison.Ordinal ) == 0 )
                    {
                        var stream = entry.Open();
                        var reader = new StreamReader( stream );
                        return reader.ReadToEnd();
                    }
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return "";
            }

            return "";
        }

        public bool write_file( string filename, string contents )
        {
            try
            {
                File.WriteAllText( filename, contents );
                return true;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return false;
            }
        }

        public string get_process_output( string processname, string arguments, int waittime = 3000 )
        {
            try
            {
                var myProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = "\"" + processname + "\"",
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    }
                };

                myProcess.Start();
                var result = myProcess.StandardOutput.ReadToEnd() + myProcess.StandardError.ReadToEnd();

                var running = true;

                do
                {
                    myProcess.Refresh();

                    if ( myProcess.HasExited )
                    {
                        running = false;
                    }

                    if ( myProcess.WaitForExit( waittime ) )
                    {
                        continue;
                    }

                    myProcess.Kill();
                    running = false;
                }
                while ( running );

                return result;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return "";
            }
        }

        public int run_process_foreground( string processname, string arguments )
        {
            try
            {
                var myProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = "\"" + processname + "\"",
                        Arguments = arguments,
                        UseShellExecute = true,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        WindowStyle = ProcessWindowStyle.Normal,
                        CreateNoWindow = false
                    }
                };

                myProcess.Start();

                var running = true;

                do
                {
                    myProcess.Refresh();

                    if ( myProcess.HasExited )
                    {
                        running = false;
                    }

                    if ( myProcess.WaitForExit( 10800000 ) )
                    {
                        continue;
                    }

                    myProcess.Kill();
                    running = false;
                }
                while ( running );

                return myProcess.ExitCode;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return 1;
            }
        }

        public int run_process_background( string processname, string arguments )
        {
            try
            {
                var myProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = "\"" + processname + "\"",
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true
                    }
                };

                myProcess.Start();

                var running = true;

                do
                {
                    myProcess.Refresh();

                    if ( myProcess.HasExited )
                    {
                        running = false;
                    }

                    if ( myProcess.WaitForExit( 10800000 ) )
                    {
                        continue;
                    }

                    myProcess.Kill();
                    running = false;
                }
                while ( running );

                return myProcess.ExitCode;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return 1;
            }
        }

        public bool check_for_internet_connection()
        {
            return this.check_for_website( "http://www.google.com" );
        }

        public bool check_for_eztv_connection()
        {
            return this.check_for_website( "https://eztv.ag" );
        }

        public bool check_for_tvdb_connection()
        {
            return this.check_for_website( "http://thetvdb.com" );
        }

        public bool check_for_website( string url )
        {
            try
            {
                using ( var client = new WebClient() )
                {
                    if ( string.Compare( Factory.Instance.Settings.ProxyServerHost, "", StringComparison.Ordinal ) != 0 )
                    {
                        var wp = new WebProxy( Factory.Instance.Settings.ProxyServerHost + ":" + Factory.Instance.Settings.ProxyServerPort, true );
                        client.Proxy = wp;
                    }
                    else
                    {
                        client.Proxy = null;
                    }

                    using ( client.OpenRead( url ) )
                    {
                        return true;
                    }
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return false;
            }
        }

        public void download_torrent( string magneturl )
        {
            try
            {
                Process.Start( magneturl );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void open_file( string file )
        {
            try
            {
                Process.Start( file );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public T get_visual_parent<T>( Visual referencedVisual ) where T : Visual
        {
            var parent = referencedVisual;

            while ( parent != null && !ReferenceEquals( parent.GetType(), typeof (T) ) )
            {
                parent = VisualTreeHelper.GetParent( parent ) as Visual;
            }

            return parent as T;
        }

        public T get_visual_child<T>( Visual referencedVisual ) where T : Visual
        {
            var parent = referencedVisual;

            if ( parent == null )
            {
                return null;
            }

            for ( var x = 0; x < VisualTreeHelper.GetChildrenCount( parent ); x++ )
            {
                var child = VisualTreeHelper.GetChild( parent, x );

                if ( ReferenceEquals( child.GetType(), typeof (StackPanel) ) )
                {
                    var sp = child as StackPanel;

                    if ( sp == null )
                    {
                        continue;
                    }

                    foreach ( var spchild in sp.Children )
                    {
                        return this.get_visual_child< T >( spchild as Visual );
                    }
                }
                else if ( ReferenceEquals( child.GetType(), typeof (T) ) )
                {
                    return child as T;
                }
            }

            return null;
        }

        public List< Visual > AllChildren( DependencyObject parent )
        {
            var list = new List< Visual >();
            for ( var i = 0; i < VisualTreeHelper.GetChildrenCount( parent ); i++ )
            {
                var child = VisualTreeHelper.GetChild( parent, i );
                if ( child is Visual )
                {
                    list.Add( child as Visual );
                }
                list.AddRange( this.AllChildren( child ) );
            }
            return list;
        }
    }
}