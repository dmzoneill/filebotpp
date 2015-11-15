using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using FileBotPP.Helpers;
using FileBotPP.Tree;
using MahApps.Metro.Controls;

namespace FileBotPP
{
    /// <summary>
    ///     Interaction logic for UserControlSettings.xaml
    /// </summary>
    public partial class UserControlSettings
    {
        public UserControlSettings()
        {
            this.InitializeComponent();
        }

        private void FileBotPpSettings_OnLoaded( object sender, RoutedEventArgs e )
        {
            try
            {
                this.ProxyHostTextBox.Text = Factory.Instance.Settings.ProxyServerHost;
                this.TvdbApiKeyTextBox.Text = Factory.Instance.Settings.TvdbApiKey;
                this.FFmpegConvertTextBox.Text = Factory.Instance.Settings.FFmpegConvert;
                this.ProxyPortTextBox.Text = Factory.Instance.Settings.ProxyServerPort == 0 ? "" : Factory.Instance.Settings.ProxyServerPort.ToString();
                this.AllowedtypesTextBox.Text = String.Join( ",", Factory.Instance.Settings.AllowedTypes );
                this.CacheTimeoutTextBox.Text = Factory.Instance.Settings.CacheTimeout.ToString();
                this.TorrentQualityComboBox.SelectedIndex = load_torrent_quality();
                this.PoorQualityComboBox.SelectedIndex = load_video_quality();
                this.load_fswatcher_values();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private static int load_torrent_quality()
        {
            var tq = Factory.Instance.Settings.TorrentPreferredQuality;

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

        private void load_fswatcher_values()
        {
            var min = Factory.Instance.Settings.FilesSystemWatcherMinRefreshTime;
            var max = Factory.Instance.Settings.FilesSystemWatcherMaxRefreshTime;
            var enabled = Factory.Instance.Settings.FilesSystemWatcherEnabled;

            this.RangeSlider.LowerValue = min;
            this.RangeSlider.UpperValue = max;
            this.ToggleSwitch.IsChecked = enabled;
        }

        private static int load_video_quality()
        {
            var vq = Factory.Instance.Settings.PoorQualityP;

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
                    Factory.Instance.Settings.PoorQualityP = 320;
                    break;
                case 1:
                    Factory.Instance.Settings.PoorQualityP = 480;
                    break;
                case 2:
                    Factory.Instance.Settings.PoorQualityP = 640;
                    break;
                case 3:
                    Factory.Instance.Settings.PoorQualityP = 720;
                    break;
                case 4:
                    Factory.Instance.Settings.PoorQualityP = 1080;
                    break;
                default:
                    Factory.Instance.Settings.PoorQualityP = 320;
                    break;
            }
        }

        private void save_torrent_quality()
        {
            switch ( this.TorrentQualityComboBox.SelectedIndex )
            {
                case 0:
                    Factory.Instance.Settings.TorrentPreferredQuality = "Normal";
                    break;
                case 1:
                    Factory.Instance.Settings.TorrentPreferredQuality = "720p";
                    break;
                case 2:
                    Factory.Instance.Settings.TorrentPreferredQuality = "1080p";
                    break;
                default:
                    Factory.Instance.Settings.TorrentPreferredQuality = "720p";
                    break;
            }
        }

        private void save_allowed_types()
        {
            var text = this.AllowedtypesTextBox.Text;

            if ( text.Contains( "," ) )
            {
                Factory.Instance.Settings.AllowedTypes = text.Split( ',' ).ToList();
                return;
            }

            Factory.Instance.Settings.AllowedTypes = new List< string > {text};
        }

        private void SaveButton_OnClick( object sender, RoutedEventArgs e )
        {
            this.save_torrent_quality();
            this.save_video_quality();
            this.save_allowed_types();

            Factory.Instance.Settings.ProxyServerHost = this.ProxyHostTextBox.Text;
            Factory.Instance.Settings.TvdbApiKey = this.TvdbApiKeyTextBox.Text;
            Factory.Instance.Settings.FFmpegConvert = this.FFmpegConvertTextBox.Text;
            Factory.Instance.Settings.CacheTimeout = int.Parse( this.CacheTimeoutTextBox.Text );
            Factory.Instance.Settings.TvdbApiKey = this.TvdbApiKeyTextBox.Text;
            Factory.Instance.Settings.FFmpegConvert = this.FFmpegConvertTextBox.Text;

            Factory.Instance.Settings.FilesSystemWatcherMaxRefreshTime = ( int ) this.RangeSlider.UpperValue;
            Factory.Instance.Settings.FilesSystemWatcherMinRefreshTime = ( int ) this.RangeSlider.LowerValue;

            var lastvalue = Factory.Instance.Settings.FilesSystemWatcherEnabled;

            Factory.Instance.Settings.FilesSystemWatcherEnabled = this.ToggleSwitch.IsChecked ?? false;

            if ( this.ToggleSwitch.IsChecked == false )
            {
                FsPoller.stop_all();
            }

            if ( lastvalue == false && Factory.Instance.Settings.FilesSystemWatcherEnabled )
            {
                FsPoller.start_all();
            }


            if ( this.test_conection() )
            {
                Factory.Instance.Settings.ProxyServerHost = this.ProxyHostTextBox.Text;
                Factory.Instance.Settings.ProxyServerPort = this.ProxyPortTextBox.Text.Length > 0 ? int.Parse( this.ProxyPortTextBox.Text ) : 0;
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
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

        private void RangeSlider_OnLowerValueChanged( object sender, RangeParameterChangedEventArgs e )
        {
            try
            {
                this.LabelLowerText.Text = this.RangeSlider.LowerValue.ToString( CultureInfo.CurrentCulture );
            }
            catch
            {
                // handled
            }
        }

        private void RangeSlider_OnUpperValueChanged( object sender, RangeParameterChangedEventArgs e )
        {
            try
            {
                this.LabelUpperText.Text = this.RangeSlider.UpperValue.ToString( CultureInfo.CurrentCulture );
            }
            catch
            {
                // handled
            }
        }
    }
}