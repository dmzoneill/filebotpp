using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FileBotPP.Helpers;
using FileBotPP.Interfaces;
using FileBotPP.Metadata;
using FileBotPP.Panes;
using FileBotPP.Tree;
using FileBotPP.Tree.Interfaces;
using Ookii.Dialogs;

namespace FileBotPP
{
    /// <summary>
    ///     Interaction logic for FileBotPPWindow.xaml
    /// </summary>
    public partial class FileBotPpWindow : IFileBotPpWindow
    {
        private static FileBotPpWindow _instance;
        private IFileItem _item;
        private DispatcherTimer _timer;

        public FileBotPpWindow()
        {
            this.InitializeComponent();

            Common.FileBotPp = this;
            _instance = this;
        }

        #region Public Setters

        private void update_log_console()
        {
            string value;
            while ( Utils.LogLines.TryDequeue( out value ) )
            {
                this.LogTextBox.AppendText( value + Environment.NewLine );
                this.LogScroller.ScrollToEnd();
            }
        }

        public void set_status_text( string text )
        {
            this.StatusLabel.Visibility = Visibility.Visible;
            this.Status.Content = text;
        }

        public void set_eztv_progress( string text )
        {
            this.EzTvLabel.Visibility = Visibility.Visible;
            this.EzTvProgress.Content = text;
        }

        public void set_tvdb_progress( string text )
        {
            this.TvDbLabel.Visibility = Visibility.Visible;
            this.TvDbProgress.Content = text;
        }

        public void set_season_count( string text )
        {
            this.SeasonCountLabel.Visibility = Visibility.Visible;
            this.SeasonCount.Content = text;
        }

        public void set_series_count( string text )
        {
            this.SeriesCountLabel.Visibility = Visibility.Visible;
            this.SeriesCount.Content = text;
        }

        public void set_episode_count( string text )
        {
            this.EpisodeCountLabel.Visibility = Visibility.Visible;
            this.EpisodeCount.Content = text;
        }

        #endregion

        #region Tree Download Actions

        private void DownloadButton_Click( object sender, RoutedEventArgs e )
        {
            this.download_torrents();
        }

        private void DownloadAllButton_OnClick( object sender, RoutedEventArgs e )
        {
            foreach ( var dir in ItemProvider.Items.OfType< IDirectoryItem >() )
            {
                foreach ( var subdir in dir.Items.OfType< IDirectoryItem >().Where( sdir => sdir.Torrent ) )
                {
                    foreach ( var tfile in subdir.Items.OfType< IFileItem >().Where( fitem => fitem.Torrent ) )
                    {
                        Utils.download_torrent( tfile.TorrentLink );
                    }
                }
            }
        }

        private void DownloadTorrentMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.download_torrents();
        }

        private void download_torrents()
        {
            if ( this.SeriesTreeView.SelectedItem == null )
            {
                return;
            }

            var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( directory != null )
            {
                if ( directory.Parent == null )
                {
                    foreach ( var dir in directory.Items.OfType< IDirectoryItem >() )
                    {
                        foreach ( var tfile in dir.Items.OfType< IFileItem >().Where( fitem => fitem.Torrent ) )
                        {
                            Utils.download_torrent( tfile.TorrentLink );
                        }
                    }
                }
                else
                {
                    foreach ( var source in directory.Items.OfType< IFileItem >().Where( fitem => fitem.Torrent ) )
                    {
                        Utils.download_torrent( source.TorrentLink );
                    }
                }

                return;
            }

            if ( file == null )
            {
                return;
            }


            if ( file.Torrent == false )
            {
                return;
            }

            Utils.download_torrent( file.TorrentLink );
        }

        #endregion

        #region Tree Convert Actions

        private void ConvertFilesMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.convert_files();
        }

        private void ConvertButton_OnClick( object sender, RoutedEventArgs e )
        {
            this.convert_files();
        }

        private void convert_files()
        {
            if ( this.SeriesTreeView.SelectedItem == null )
            {
                return;
            }

            var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( directory != null )
            {
                var fmp1 = new FfmpegConvertWorker( directory );
                fmp1.start_convert();
                Common.Working.Add( fmp1 );
            }

            if ( file == null )
            {
                return;
            }

            var fmp2 = new FfmpegConvertWorker( file );
            fmp2.start_convert();
            Common.Working.Add( fmp2 );
        }

        #endregion

        #region Tree Delete Actions

        private void DeleteFileMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.delete_files();
        }

        private void DeleteFolderMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.delete_folder();
        }

        private void DeleteMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.delete_file();
        }

        private void DeleteButton_Click( object sender, RoutedEventArgs e )
        {
            this.delete_files();
        }

        private void delete_files()
        {
            if ( this.SeriesTreeView.SelectedItem == null )
            {
                return;
            }

            var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( directory != null )
            {
                ItemProvider.delete_invalid_folder( directory );
            }

            if ( file == null )
            {
                return;
            }

            ItemProvider.delete_invalid_file( file );
        }

        private void delete_file()
        {
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( file == null )
            {
                return;
            }

            ItemProvider.delete_file( file );
        }

        private void delete_folder()
        {
            var folder = this.SeriesTreeView.SelectedItem as IDirectoryItem;

            if ( folder == null )
            {
                return;
            }

            ItemProvider.delete_folder( folder );
        }

        #endregion

        #region Tree Check File Name Actions

        private void CheckNamesMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.check_names();
        }

        private void RenameFilesButton_Click( object sender, RoutedEventArgs e )
        {
            var button = sender as Button;
            if ( button == null )
            {
                return;
            }

            var treeViewItem = Utils.get_visual_parent< TreeViewItem >( button );

            if ( treeViewItem == null )
            {
                return;
            }

            var fitem = treeViewItem.Header as IFileItem;
            if ( fitem != null )
            {
                ItemProvider.rename_file_item( fitem );
            }
            else
            {
                var header = treeViewItem.Header as IDirectoryItem;

                if ( header != null )
                {
                    ItemProvider.rename_directory_items( header );
                }
            }
        }

        private void CheckNamesButton_Click( object sender, RoutedEventArgs e )
        {
            this.check_names();
        }

        private void CheckNamesAllButton_OnClick( object sender, RoutedEventArgs e )
        {
            check_names_all();
        }

        private void check_names()
        {
            if ( this.SeriesTreeView.SelectedItem == null )
            {
                return;
            }

            Common.Filebot = new Filebot();

            var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( directory != null )
            {
                if ( directory.Parent != null )
                {
                    Common.Filebot.check_series( ( IDirectoryItem ) directory.Parent );
                }
                else
                {
                    Common.Filebot.check_series( directory );
                }
            }
            else
            {
                if ( file?.Parent?.Parent != null )
                {
                    Common.Filebot.check_series( ( IDirectoryItem ) file.Parent.Parent );
                }
            }
        }

        private static void check_names_all()
        {
            Common.Filebot?.stop_worker();
            Common.Filebot = new Filebot();
            Common.Filebot.check_series_all();
        }

        #endregion

        #region Tree Move File Actions

        private void MoveButton_Click( object sender, RoutedEventArgs e )
        {
            this.move_files();
        }

        private void MoveItemsMenuIem_OnClick( object sender, RoutedEventArgs e )
        {
            this.move_files();
        }

        private void move_files()
        {
            if ( this.SeriesTreeView.SelectedItem == null )
            {
                return;
            }

            Common.FileBotPp.set_status_text( "Moving files to correct folders" );
            Utils.LogLines.Enqueue( "Moving files to correct folders" );

            var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( directory != null )
            {
                ItemProvider.move_files_to_valid_folders( directory );
            }

            if ( file == null )
            {
                return;
            }

            ItemProvider.move_file_to_valid_folder( file );

            Common.FileBotPp.set_status_text( "Files moved, see log for details" );
            Utils.LogLines.Enqueue( "Files moved, see log for details" );
        }

        #endregion

        #region Tree Check Corrupt File Actions

        private void CheckAllFilesButton_Click( object sender, RoutedEventArgs e )
        {
            var fmi = new MediaInfoWorker();
            fmi.start_scan();
            Common.Working.Add( fmi );

            var ffmpeg = new FfmpegCorruptWorker();
            ffmpeg.start_scan();
            Common.Working.Add( ffmpeg );
        }

        private void CheckButton_Click( object sender, RoutedEventArgs e )
        {
            this.check_files_corruption();
        }

        private void check_files_corruption()
        {
            if ( this.SeriesTreeView.SelectedItem == null )
            {
                return;
            }

            var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( directory != null )
            {
                var dmi = new MediaInfoWorker( directory );
                dmi.start_scan();

                var ffmpeg = new FfmpegCorruptWorker( directory );
                ffmpeg.start_scan();
            }

            if ( file == null )
            {
                return;
            }

            var fmi = new MediaInfoWorker( file );
            fmi.start_scan();

            var ffmpegfile = new FfmpegCorruptWorker( file );
            ffmpegfile.start_scan();
        }

        private void CheckCorruptMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.check_files_corruption();
        }

        #endregion

        #region Window Events

        private void Window_Closing( object sender, CancelEventArgs e )
        {
            this._timer.Stop();
        }

        private void _timer_Tick( object sender, EventArgs e )
        {
            this.update_log_console();
        }

        private void Window_Loaded( object sender, RoutedEventArgs e )
        {
            this._timer = new DispatcherTimer();
            this._timer.Tick += this._timer_Tick;
            this._timer.Interval = new TimeSpan( 0, 0, 1 );
            this._timer.Start();

            this.CheckConnections();
        }

        private void CheckConnections()
        {
            this.set_status_text( "Checking for internet access.." );
            var connectionChecker = new BackgroundWorker();
            connectionChecker.DoWork += connectionChecker_DoWork;
            connectionChecker.RunWorkerCompleted += this.connectionChecker_RunWorkerCompleted;
            connectionChecker.RunWorkerAsync();
        }

        private void connectionChecker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            this.SelectFolderButton.IsEnabled = true;

            this.set_status_text( "" );

            if ( Common.EztvAvailable )
            {
                Common.SeriesAnalyzer.fetch_eztv_metadata();
                return;
            }

            MessageBox.Show( "Unable to access eztv." + Environment.NewLine + "This functionality will be disabled", "Connectivity Issues" );
        }

        private static void connectionChecker_DoWork( object sender, DoWorkEventArgs e )
        {
            Common.EztvAvailable = Utils.check_for_eztv_connection();
            Common.TvdbAvailable = Utils.check_for_tvdb_connection();
        }

        private void SeriesTreeView_SelectedItemChanged( object sender, RoutedPropertyChangedEventArgs< object > e )
        {
            if ( this.SeriesTreeView.SelectedItem == null )
            {
                return;
            }

            var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( directory != null )
            {
                var fname = ItemProvider.get_series_name_from_folder( directory );
                var series = Common.Tvdb.get_series_by_name( fname );

                if ( series != null )
                {
                    this.TvdbTab.Content = new SeriesViewer( series );
                }
                else
                {
                    this.TvdbTab.Content = new QuestionMissing();
                }
            }

            if ( file == null )
            {
                return;
            }

            var name = ItemProvider.get_series_name_from_file( file );

            if ( name != null )
            {
                var series = Common.Tvdb.get_series_by_name( name );
                if ( series != null )
                {
                    this.TvdbTab.Content = new SeriesViewer( series );
                }
                else
                {
                    this.TvdbTab.Content = new QuestionMissing();
                }
            }
            else
            {
                this.TvdbTab.Content = new QuestionMissing();
            }

            if ( file.Missing )
            {
                this.MediaTab.Content = new QuestionMissing();
                return;
            }

            MediaInfoWorker.scan_file_one_time( file );

            if ( file.Mediainfo == null )
            {
                this.MediaTab.Content = new QuestionMissing();
                return;
            }

            this._item = file;

            var fetchMetaDataWorker = new BackgroundWorker();
            fetchMetaDataWorker.DoWork += this.fetchMetaDataWorker_DoWork;
            fetchMetaDataWorker.RunWorkerCompleted += this.fetchMetaDataWorker_RunWorkerCompleted;
            fetchMetaDataWorker.RunWorkerAsync();
        }

        private void SeriesTreeView_MouseRightButtonDown( object sender, MouseButtonEventArgs e )
        {
            var item = sender as TreeViewItem;
            if ( item == null )
            {
                return;
            }

            item.Focus();
            e.Handled = true;
        }

        private void SelectFolderButton_Click( object sender, RoutedEventArgs e )
        {
            var dialog = new VistaFolderBrowserDialog {Description = @"Please select your series folder.", UseDescriptionForTitle = true};

            var result = dialog.ShowDialog();

            if ( result != System.Windows.Forms.DialogResult.OK )
            {
                return;
            }

            this.set_ready( false );

            if ( Common.MetaDataReady == 3 )
            {
                Common.MetaDataReady = 1;
            }

            this.PathLabel.Visibility = Visibility.Visible;

            Common.ScanLocation = dialog.SelectedPath;
            this.ScanLocationLabel.Content = dialog.SelectedPath;

            ItemProvider.Items.Clear();
            this.SeriesTreeView.DataContext = ItemProvider.Items;
            ItemProvider.scan_series_folder();

            if ( Common.TvdbAvailable == false )
            {
                MessageBox.Show( "Unable to access thetvdb." + Environment.NewLine + "This functionality will be disabled", "Connectivity Issues" );
                return;
            }

            Common.SeriesAnalyzer.fetch_tvdb_metadata();
        }

        private void fetchMetaDataWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            this.MediaTab.Content = new MediaInfoViewer( this._item.Mediainfo, this._item.FullName );
        }

        private void fetchMetaDataWorker_DoWork( object sender, DoWorkEventArgs e )
        {
            if ( this._item.Mediainfo != null )
            {
                return;
            }

            MediaInfoWorker.scan_file_one_time( this._item );
        }

        private void OpenFolderMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( directory != null )
            {
                Utils.open_file( directory.Path );
            }

            if ( file == null )
            {
                return;
            }

            Utils.open_file( file.Parent.Path );
        }

        private void PlayFileMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            var file = this.SeriesTreeView.SelectedItem as IFileItem;

            if ( file == null )
            {
                return;
            }

            Utils.open_file( file.Path );
        }

        private void ExitMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            Common.stop_all_workers();
            this.Close();
        }

        private void StopThreadsMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            Common.stop_all_workers();
        }

        public void set_ready( bool ready )
        {
            this.SeriesTreeView.IsEnabled = ready;
            this.CheckAllFilesButton.IsEnabled = ready;
            this.CheckNamesButton.IsEnabled = ready;
            this.MoveButton.IsEnabled = ready;
            this.DeleteButton.IsEnabled = ready;
            this.CheckButton.IsEnabled = ready;
            this.ConvertButton.IsEnabled = ready;
            this.DownloadButton.IsEnabled = ready;
            this.CheckNamesAllButton.IsEnabled = ready;
            this.DownloadAllButton.IsEnabled = ready;
        }

        #endregion
    }
}