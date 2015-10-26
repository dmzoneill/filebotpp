using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using FileBotPP.Helpers;

namespace FileBotPP.Panes
{
    /// <summary>
    ///     Interaction logic for FileBotPpSettings.xaml
    /// </summary>
    public partial class FileBotPpSettings
    {
        public FileBotPpSettings()
        {
            this.InitializeComponent();
        }

        private void FileBotPpSettings_OnLoaded( object sender, RoutedEventArgs e )
        {
            try
            {
                this.ProxyPortTextBox.Text = Settings.ProxyServerPort == 0 ? "" : Settings.ProxyServerPort.ToString();
                this.AllowedtypesTextBox.Text = String.Join( ",", Settings.AllowedTypes );
                this.CacheTimeoutTextBox.Text = Settings.CacheTimeout.ToString();
                this.TorrentQualityComboBox.SelectedIndex = load_torrent_quality();
                this.PoorQualityComboBox.SelectedIndex = load_video_quality();
            }
            catch (Exception ex)
            {
                Utils.LogLines.Enqueue(ex.Message);
                Utils.LogLines.Enqueue(ex.StackTrace);
            }
        }

        private static int load_torrent_quality()
        {
            var tq = Settings.TorrentPreferredQuality;

            switch ( tq )
            {
                case "Normal":
                    return 0;
                case "720p":
                    return 1;
                case "1080p":
                    return 2;
                default:
                    return 1;
            }
        }

        private static int load_video_quality()
        {
            var vq = Settings.PoorQualityP;

            switch ( vq )
            {
                case 320:
                    return 0;
                case 480:
                    return 1;
                case 640:
                    return 2;
                case 720:
                    return 3;
                case 1080:
                    return 4;
                default:
                    return 0;
            }
        }

        private void save_video_quality()
        {
            switch ( this.PoorQualityComboBox.SelectedIndex )
            {
                case 0:
                    Settings.PoorQualityP = 320;
                    break;
                case 1:
                    Settings.PoorQualityP = 480;
                    break;
                case 2:
                    Settings.PoorQualityP = 640;
                    break;
                case 3:
                    Settings.PoorQualityP = 720;
                    break;
                case 4:
                    Settings.PoorQualityP = 1080;
                    break;
                default:
                    Settings.PoorQualityP = 320;
                    break;
            }
        }

        private void save_torrent_quality()
        {
            switch ( this.TorrentQualityComboBox.SelectedIndex )
            {
                case 0:
                    Settings.TorrentPreferredQuality = "Normal";
                    break;
                case 1:
                    Settings.TorrentPreferredQuality = "720p";
                    break;
                case 2:
                    Settings.TorrentPreferredQuality = "1080p";
                    break;
                default:
                    Settings.TorrentPreferredQuality = "720p";
                    break;
            }
        }

        private void save_allowed_types()
        {
            var text = this.AllowedtypesTextBox.Text;

            if ( text.Contains( "," ) )
            {
                Settings.AllowedTypes = text.Split( ',' ).ToList();
                return;
            }

            Settings.AllowedTypes = new List< string > {text};
        }

        private void SaveButton_OnClick( object sender, RoutedEventArgs e )
        {
            this.save_torrent_quality();
            this.save_video_quality();
            this.save_allowed_types();

            Settings.CacheTimeout = int.Parse( this.CacheTimeoutTextBox.Text );
            Settings.TvdbApiKey = this.TvdbApiKeyTextBox.Text;
            Settings.FFmpegConvert = this.FFmpegConvertTextBox.Text;

            if ( this.test_conection() )
            {
                Settings.ProxyServerHost = this.ProxyHostTextBox.Text;
                Settings.ProxyServerPort = this.ProxyPortTextBox.Text.Length > 0 ? int.Parse( this.ProxyPortTextBox.Text ) : 0;
            }
            else
            {
                MessageBox.Show( "Unable to connect to " + this.ProxyHostTextBox.Text + ":" + this.ProxyPortTextBox.Text + Environment.NewLine + "Proxy Settings not saved", "Conection error" );
            }
        }

        private bool test_conection()
        {
            var client = new TcpClient();
            try
            {
                if ( String.Compare( this.ProxyHostTextBox.Text, "", StringComparison.Ordinal ) == 0 )
                {
                    return true;
                }
                client.Connect( this.ProxyHostTextBox.Text, int.Parse( this.ProxyPortTextBox.Text ) );
                return true;
            }
            catch (Exception ex)
            {
                Utils.LogLines.Enqueue(ex.Message);
                Utils.LogLines.Enqueue(ex.StackTrace);
                return false;
            }
        }

        private void NumericTextBox_PreviewTextInput( object sender, TextCompositionEventArgs e )
        {
            e.Handled = !IsTextAllowed( e.Text );
        }

        private static bool IsTextAllowed( string text )
        {
            var regex = new Regex( "[^0-9.-]+" ); //regex that matches disallowed text
            return !regex.IsMatch( text );
        }
    }
}