using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using FileBotPP.Helpers;
using FileBotPP.Interfaces;

namespace FileBotPP.Panes
{
    /// <summary>
    ///     Interaction logic for SeriesViewer.xaml
    /// </summary>
    public partial class SeriesViewer : INotifyPropertyChanged
    {
        public SeriesViewer()
        {
            this.DataContext = this.Series;
            this.InitializeComponent();
        }

        public SeriesViewer( ISeries series )
        {
            this.Series = series;
            this.InitializeComponent();
        }

        public ISeries Series { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged( string propertyName )
        {
            this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        private void UserControl_Loaded( object sender, RoutedEventArgs e )
        {
            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri( "http://thetvdb.com/banners/_cache/" + this.Series.Poster );
                bitmapImage.EndInit();

                this.TvseriesImage.Source = bitmapImage;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }

            this.create_torrent_hyperlinks( false );
        }

        private void Hyperlink_RequestNavigate( object sender, RequestNavigateEventArgs e )
        {
            try
            {
                Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) );
                e.Handled = true;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        // ReSharper disable once FunctionComplexityOverflow
        private void create_torrent_hyperlinks( bool download )
        {
            try
            {
                var doc = new FlowDocument();

                var para = new Paragraph();
                doc.Blocks.Add( para );

                foreach ( var torrent in Common.Eztv.get_torrents() )
                {
                    if ( String.Compare( torrent.Imbdid, this.Series.ImdbId, StringComparison.Ordinal ) != 0 )
                    {
                        continue;
                    }

                    if ( this.CheckBoxHdtv.IsChecked ?? false )
                    {
                        if ( torrent.Epname.ToLower().Contains( "hdtv" ) && !torrent.Epname.ToLower().Contains( "720" ) && !torrent.Epname.ToLower().Contains( "1080" ) )
                        {
                            continue;
                        }
                    }

                    if ( this.CheckBox720P.IsChecked ?? false )
                    {
                        if ( torrent.Epname.ToLower().Contains( "720" ) )
                        {
                            continue;
                        }
                    }

                    if ( this.CheckBox1080P.IsChecked ?? false )
                    {
                        if ( torrent.Epname.ToLower().Contains( "1080" ) )
                        {
                            continue;
                        }
                    }

                    var textblock = new TextBlock {Text = torrent.Epname, TextWrapping = TextWrapping.NoWrap};

                    var link = new Hyperlink {IsEnabled = true};
                    link.Inlines.Add( textblock );
                    link.NavigateUri = new Uri( torrent.Magnetlink );
                    link.Click += Link_Click;

                    para.Inlines.Add( link );
                    para.Inlines.Add( Environment.NewLine );

                    if ( download )
                    {
                        Utils.download_torrent( link.NavigateUri.ToString() );
                    }
                }

                this.RichTextTorrents.Document = doc;
                this.RichTextTorrents.IsReadOnly = true;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private static void Link_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                var link = e.OriginalSource as Hyperlink;

                if ( link == null )
                {
                    return;
                }

                Utils.download_torrent( link.NavigateUri.ToString() );
                e.Handled = true;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void CheckBoxHdtv_OnClick( object sender, RoutedEventArgs e )
        {
            this.create_torrent_hyperlinks( false );
        }

        private void CheckBox720P_OnClick( object sender, RoutedEventArgs e )
        {
            this.create_torrent_hyperlinks( false );
        }

        private void CheckBox1080P_OnClick( object sender, RoutedEventArgs e )
        {
            this.create_torrent_hyperlinks( false );
        }

        private void DownloadAllButton_OnClick( object sender, RoutedEventArgs e )
        {
            this.create_torrent_hyperlinks( true );
        }
    }
}