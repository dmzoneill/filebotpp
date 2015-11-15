using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using FileBotPP.Helpers;
using FileBotPP.Metadata;

namespace FileBotPP
{
    /// <summary>
    ///     Interaction logic for UserControlSeriesViewer.xaml
    /// </summary>
    public partial class UserControlSeriesViewer : INotifyPropertyChanged
    {
        private BitmapImage _seriesImage1;

        public UserControlSeriesViewer()
        {
            this.DataContext = this.TvdbSeries;
            this.InitializeComponent();
        }

        public UserControlSeriesViewer( ITvdbSeries tvdbSeries )
        {
            this.TvdbSeries = tvdbSeries;
            this.InitializeComponent();
        }

        public ITvdbSeries TvdbSeries { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged( string propertyName )
        {
            this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        private void UserControl_Loaded( object sender, RoutedEventArgs e )
        {
            try
            {
                if ( File.Exists( Factory.Instance.AppDataFolder + "/tvdbartwork/poster/" + this.TvdbSeries.Id + ".jpg" ) )
                {
                    this._seriesImage1 = new BitmapImage();
                    this._seriesImage1.BeginInit();
                    this._seriesImage1.UriSource = new Uri( Factory.Instance.AppDataFolder + "/tvdbartwork/poster/" + this.TvdbSeries.Id + ".jpg" );
                    this._seriesImage1.EndInit();
                    this.TvseriesImage.Source = this._seriesImage1;
                }
                else
                {
                    this._seriesImage1 = new BitmapImage();
                    this._seriesImage1.BeginInit();
                    this._seriesImage1.UriSource = new Uri( "http://thetvdb.com/banners/_cache/" + this.TvdbSeries.Poster );
                    this._seriesImage1.EndInit();
                    this.TvseriesImage.Source = this._seriesImage1;
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                foreach ( var torrent in Factory.Instance.Eztv.get_torrents().Where( torrent => String.Compare( torrent.Imbdid, this.TvdbSeries.ImdbId, StringComparison.Ordinal ) == 0 ) )
                {
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
                    link.Click += this.Link_Click;

                    para.Inlines.Add( link );
                    para.Inlines.Add( Environment.NewLine );

                    if ( download )
                    {
                        Factory.Instance.Utils.download_torrent( link.NavigateUri.ToString() );
                    }
                }

                this.RichTextTorrents.Document = doc;
                this.RichTextTorrents.IsReadOnly = true;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void Link_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                var link = e.OriginalSource as Hyperlink;

                if ( link == null )
                {
                    return;
                }

                Factory.Instance.Utils.download_torrent( link.NavigateUri.ToString() );
                e.Handled = true;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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