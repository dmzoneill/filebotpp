using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
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
        private IFileItem _item;
        private TreeViewItem _lastSelected;
        private DispatcherTimer _timer;

        public FileBotPpWindow()
        {
            this.InitializeComponent();

            Common.FileBotPp = this;
            this.SettingsScrollViewer.Content = new FileBotPpSettings();
        }

        #region Tree refresh folder

        private void RefreshMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            try
            {
                var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;

                if ( directory == null )
                {
                    return;
                }

                directory.Items.Clear();
                ItemProvider.refresh_tree_directory( directory, directory.Path );
                ItemProvider.folder_scan_update();
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        #endregion

        #region Tree add series

        private void AddSeriesMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.AddSeries();
        }

        private void AddSeriesButton_OnClick( object sender, RoutedEventArgs e )
        {
            this.AddSeries();
        }

        private void AddSeries()
        {
            try
            {
                var asw = new AddSeriesWindow {Top = this.Top + ( this.Height/2 ) - 30, Left = this.Left + ( this.Width/2 ) - 225};
                asw.ShowDialog();

                if ( Common.AddSeriesName == null )
                {
                    return;
                }

                Directory.CreateDirectory( Common.ScanLocation + "\\" + Common.AddSeriesName );

                Common.Tvdb.downloads_series_data( Common.AddSeriesName );

                Thread.Sleep( 100 );

                var newseries = new DirectoryItem {FullName = Common.AddSeriesName, Path = Common.ScanLocation + "\\" + Common.AddSeriesName, Polling = true};
                ItemProvider.insert_item_ordered( newseries );

                var item = this.SeriesTreeView.ItemContainerGenerator.ContainerFromItem( newseries ) as TreeViewItem;

                if ( item == null )
                {
                    return;
                }

                item.IsSelected = true;
                item.BringIntoView();

                Common.SeriesAnalyzer.analyze_series_folder( newseries );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        #endregion

        #region Tree Rename folder names

        private void RenameDirectoryMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            try
            {
                if ( this._lastSelected == null )
                {
                    return;
                }

                var children = Utils.AllChildren( this._lastSelected );
                var outersp = children.OfType< StackPanel >().First();
                outersp.Children[ 2 ].Visibility = Visibility.Collapsed;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void DirectoryNameTextBlock_MouseDown( object sender, MouseButtonEventArgs e )
        {
            try
            {
                if ( e.ClickCount != 2 )
                {
                    return;
                }

                var textblock = sender as TextBlock;

                if ( textblock == null )
                {
                    return;
                }

                textblock.Visibility = Visibility.Collapsed;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void FolderRenameTextBox_PreviewKeyDown( object sender, KeyEventArgs e )
        {
            try
            {
                var textbox = sender as TextBox;

                if ( textbox == null )
                {
                    return;
                }

                var treeViewItem = Utils.get_visual_parent< TreeViewItem >( textbox );
                var diritem = treeViewItem?.Header as IDirectoryItem;

                var stackpanel = Utils.get_visual_parent< StackPanel >( textbox );
                var textblock = stackpanel.Children[ 2 ] as TextBlock;

                if ( textblock == null || diritem == null )
                {
                    return;
                }

                if ( e.Key == Key.Escape )
                {
                    textbox.Text = diritem.FullName;
                    textblock.Visibility = Visibility.Visible;
                    return;
                }

                if ( e.Key != Key.Return )
                {
                    return;
                }

                e.Handled = true;

                textblock.Visibility = Visibility.Visible;

                if ( string.Compare( textbox.Text, textblock.Text, StringComparison.Ordinal ) == 0 )
                {
                    return;
                }

                diritem.Rename( textbox.Text );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void FileNameTextBlock_OnMouseDown( object sender, MouseButtonEventArgs e )
        {
            try
            {
                if ( e.ClickCount != 2 )
                {
                    return;
                }

                var textblock = sender as TextBlock;

                if ( textblock == null )
                {
                    return;
                }

                textblock.Visibility = Visibility.Collapsed;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void RenameFileMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            try
            {
                if ( this._lastSelected == null )
                {
                    return;
                }

                var children = Utils.AllChildren( this._lastSelected );
                var outersp = children.OfType< StackPanel >().First();
                var innersp = outersp.Children.OfType< StackPanel >().First();
                innersp.Children[ 0 ].Visibility = Visibility.Collapsed;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void FileRenameTextBox_PreviewKeyDown( object sender, KeyEventArgs e )
        {
            try
            {
                var textbox = sender as TextBox;

                if ( textbox == null )
                {
                    return;
                }

                var treeViewItem = Utils.get_visual_parent< TreeViewItem >( textbox );
                var fileitem = treeViewItem?.Header as IFileItem;

                var stackpanel = Utils.get_visual_parent< StackPanel >( textbox );
                var textblock = stackpanel.Children[ 0 ] as TextBlock;

                if ( textblock == null || fileitem == null )
                {
                    return;
                }

                if ( e.Key == Key.Escape )
                {
                    textbox.Text = fileitem.FullName;
                    textblock.Visibility = Visibility.Visible;
                    return;
                }

                if ( e.Key != Key.Return )
                {
                    return;
                }

                e.Handled = true;

                textblock.Visibility = Visibility.Visible;

                if ( string.Compare( textbox.Text, textblock.Text, StringComparison.Ordinal ) == 0 )
                {
                    return;
                }

                FsPoller.stop_all();
                fileitem.Rename( textbox.Text );
                FsPoller.start_all();
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        #endregion

        #region Public Setters

        private void update_log_console()
        {
            try
            {
                var last = "";
                string value;

                while ( Utils.LogLines.TryDequeue( out value ) )
                {
                    if ( String.Compare( last, value, StringComparison.Ordinal ) == 0 )
                    {
                        continue;
                    }

                    this.LogTextBox.AppendText( value + Environment.NewLine );
                    this.LogScroller.ScrollToEnd();
                    last = value;
                }
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void set_status_text( string text )
        {
            try
            {
                this.StatusLabel.Visibility = Visibility.Visible;
                this.Status.Content = text;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void set_eztv_progress( string text )
        {
            try
            {
                this.EzTvLabel.Visibility = Visibility.Visible;
                this.EzTvProgress.Content = text;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void set_tvdb_progress( string text )
        {
            try
            {
                this.TvDbLabel.Visibility = Visibility.Visible;
                this.TvDbProgress.Content = text;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void set_season_count( string text )
        {
            try
            {
                this.SeasonCountLabel.Visibility = Visibility.Visible;
                this.SeasonCount.Content = text;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void set_series_count( string text )
        {
            try
            {
                this.SeriesCountLabel.Visibility = Visibility.Visible;
                this.SeriesCount.Content = text;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void set_episode_count( string text )
        {
            try
            {
                this.EpisodeCountLabel.Visibility = Visibility.Visible;
                this.EpisodeCount.Content = text;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void set_ready( bool ready )
        {
            try
            {
                this.SeriesTreeView.IsEnabled = ready;
                this.CheckAllFilesButton.IsEnabled = ready;
                this.DownloadAllButton.IsEnabled = ready;
                this.CheckNamesAllButton.IsEnabled = ready;
                this.AddSeriesButton.IsEnabled = ready;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        #endregion

        #region Tree Download Actions

        private void DownloadButton_Click( object sender, RoutedEventArgs e )
        {
            this.download_torrents();
        }

        private void DownloadAllButton_OnClick( object sender, RoutedEventArgs e )
        {
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void DownloadTorrentMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.download_torrents();
        }

        private void download_torrents()
        {
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
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
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
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
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void delete_file()
        {
            try
            {
                var file = this.SeriesTreeView.SelectedItem as IFileItem;

                if ( file == null )
                {
                    return;
                }

                ItemProvider.delete_file( file );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void delete_folder()
        {
            try
            {
                var folder = this.SeriesTreeView.SelectedItem as IDirectoryItem;

                if ( folder == null )
                {
                    return;
                }

                ItemProvider.delete_folder( folder );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        #endregion

        #region Tree Check File Name Actions

        private void CheckNamesMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.check_names();
        }

        private void RenameFilesButton_Click( object sender, RoutedEventArgs e )
        {
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
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
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private static void check_names_all()
        {
            try
            {
                Common.Filebot?.stop_worker();
                Common.Filebot = new Filebot();
                Common.Filebot.check_series_all();
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
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
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        #endregion

        #region Tree Check Corrupt File Actions

        private void CheckAllFilesButton_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                var fmi = new MediaInfoWorker();
                fmi.start_scan();
                Common.Working.Add( fmi );

                var ffmpeg = new FfmpegCorruptWorker();
                ffmpeg.start_scan();
                Common.Working.Add( ffmpeg );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void CheckButton_Click( object sender, RoutedEventArgs e )
        {
            this.check_files_corruption();
        }

        private void check_files_corruption()
        {
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void CheckCorruptMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            this.check_files_corruption();
        }

        #endregion

        #region Window Events

        private void Window_Closing( object sender, CancelEventArgs e )
        {
            try
            {
                this._timer.Stop();
                Common.stop_all_workers();
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void _timer_Tick( object sender, EventArgs e )
        {
            this.update_log_console();
        }

        private void Window_Loaded( object sender, RoutedEventArgs e )
        {
            try
            {
                this._timer = new DispatcherTimer();
                this._timer.Tick += this._timer_Tick;
                this._timer.Interval = new TimeSpan( 0, 0, 1 );
                this._timer.Start();

                this.CheckConnections();
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void CheckConnections()
        {
            try
            {
                this.set_status_text( "Checking for internet access.." );
                var connectionChecker = new BackgroundWorker();
                connectionChecker.DoWork += connectionChecker_DoWork;
                connectionChecker.RunWorkerCompleted += this.connectionChecker_RunWorkerCompleted;
                connectionChecker.RunWorkerAsync();
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void connectionChecker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private static void connectionChecker_DoWork( object sender, DoWorkEventArgs e )
        {
            try
            {
                Common.EztvAvailable = Utils.check_for_eztv_connection();
                Common.TvdbAvailable = Utils.check_for_tvdb_connection();
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void SeriesTreeView_SelectedItemChanged( object sender, RoutedPropertyChangedEventArgs< object > e )
        {
            try
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
                    var series = Common.Tvdb?.get_series_by_name( fname );

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
                    var series = Common.Tvdb?.get_series_by_name( name );
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void SeriesTreeView_MouseRightButtonDown( object sender, MouseButtonEventArgs e )
        {
            try
            {
                var item = sender as TreeViewItem;
                if ( item == null )
                {
                    return;
                }

                item.Focus();
                e.Handled = true;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void SelectFolderButton_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                var dialog = new VistaFolderBrowserDialog {Description = @"Please select your series folder.", UseDescriptionForTitle = true};

                var result = dialog.ShowDialog();

                if ( result != System.Windows.Forms.DialogResult.OK )
                {
                    return;
                }

                this.set_ready( false );

                if ( Common.MetaDataReady >= 3 )
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void fetchMetaDataWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            try
            {
                this.MediaTab.Content = new MediaInfoViewer( this._item.Mediainfo, this._item.FullName );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void fetchMetaDataWorker_DoWork( object sender, DoWorkEventArgs e )
        {
            try
            {
                if ( this._item.Mediainfo != null )
                {
                    return;
                }

                MediaInfoWorker.scan_file_one_time( this._item );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void OpenFolderMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            try
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
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void PlayFileMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            try
            {
                var file = this.SeriesTreeView.SelectedItem as IFileItem;

                if ( file == null )
                {
                    return;
                }

                Utils.open_file( file.Path );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void StopThreadsMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            try
            {
                Common.stop_all_workers();
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void SeriesTreeView_OnSelected( object sender, RoutedEventArgs e )
        {
            try
            {
                this._lastSelected = ( TreeViewItem ) e.OriginalSource;
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void SettingsTitleButton_OnClick( object sender, RoutedEventArgs e )
        {
            this.TabControl.SelectedIndex = 3;
        }

        #endregion
    }
}