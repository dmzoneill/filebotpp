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
using FileBotPP.Metadata;
using FileBotPP.Tree;
using Ookii.Dialogs;

namespace FileBotPP
{
    /// <summary>
    ///     Interaction logic for FileBotPPWindow.xaml
    /// </summary>
    public partial class WindowFileBotPp : IWindowFileBotPp
    {
        private IFileItem _item;
        private TreeViewItem _lastSelected;
        private DispatcherTimer _timer;

        public WindowFileBotPp()
        {
            this.InitializeComponent();

            Factory.Instance.WindowFileBotPp = this;
            this.SettingsScrollViewer.Content = new UserControlSettings();
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
                Factory.Instance.ItemProvider.refresh_tree_directory( directory, directory.Path );
                Factory.Instance.ItemProvider.folder_scan_update();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        #endregion

        #region Tree add Series

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
                var asw = new WindowAddSeries {Top = this.Top + ( this.Height/2 ) - 30, Left = this.Left + ( this.Width/2 ) - 225};
                asw.ShowDialog();

                if ( Factory.Instance.AddSeriesName == null )
                {
                    return;
                }

                Directory.CreateDirectory( Factory.Instance.ScanLocation + "\\" + Factory.Instance.AddSeriesName );

                Factory.Instance.Tvdb.downloads_series_data( Factory.Instance.AddSeriesName );

                Thread.Sleep( 100 );

                var newseries = new DirectoryItem {FullName = Factory.Instance.AddSeriesName, Path = Factory.Instance.ScanLocation + "\\" + Factory.Instance.AddSeriesName, Polling = true};
                Factory.Instance.ItemProvider.insert_item_ordered( newseries );

                var item = this.SeriesTreeView.ItemContainerGenerator.ContainerFromItem( newseries ) as TreeViewItem;

                if ( item == null )
                {
                    return;
                }

                item.IsSelected = true;
                item.BringIntoView();

                Factory.Instance.SeriesAnalyzer.analyze_series_folder( newseries );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                var children = Factory.Instance.Utils.AllChildren( this._lastSelected );
                var outersp = children.OfType< StackPanel >().First();
                outersp.Children[ 2 ].Visibility = Visibility.Collapsed;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                var treeViewItem = Factory.Instance.Utils.get_visual_parent< TreeViewItem >( textbox );
                var diritem = treeViewItem?.Header as IDirectoryItem;

                var stackpanel = Factory.Instance.Utils.get_visual_parent< StackPanel >( textbox );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                var children = Factory.Instance.Utils.AllChildren( this._lastSelected );
                var outersp = children.OfType< StackPanel >().First();
                var innersp = outersp.Children.OfType< StackPanel >().First();
                innersp.Children[ 0 ].Visibility = Visibility.Collapsed;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                var treeViewItem = Factory.Instance.Utils.get_visual_parent< TreeViewItem >( textbox );
                var fileitem = treeViewItem?.Header as IFileItem;

                var stackpanel = Factory.Instance.Utils.get_visual_parent< StackPanel >( textbox );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                while ( Factory.Instance.LogLines.TryDequeue( out value ) )
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void set_kat_progress( string text )
        {
            try
            {
                this.KatLabel.Visibility = Visibility.Visible;
                this.Katprogress.Content = text;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                foreach ( var dir in Factory.Instance.ItemProvider.Items.OfType< IDirectoryItem >() )
                {
                    foreach ( var subdir in dir.Items.OfType< IDirectoryItem >().Where( sdir => sdir.Torrent ) )
                    {
                        foreach ( var tfile in subdir.Items.OfType< IFileItem >().Where( fitem => fitem.Torrent ) )
                        {
                            Factory.Instance.Utils.download_torrent( tfile.TorrentLink );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                                Factory.Instance.Utils.download_torrent( tfile.TorrentLink );
                            }
                        }
                    }
                    else
                    {
                        foreach ( var source in directory.Items.OfType< IFileItem >().Where( fitem => fitem.Torrent ) )
                        {
                            Factory.Instance.Utils.download_torrent( source.TorrentLink );
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

                Factory.Instance.Utils.download_torrent( file.TorrentLink );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                    Factory.Instance.Working.Add( fmp1 );
                }

                if ( file == null )
                {
                    return;
                }

                var fmp2 = new FfmpegConvertWorker( file );
                fmp2.start_convert();
                Factory.Instance.Working.Add( fmp2 );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                    Factory.Instance.ItemProvider.delete_invalid_folder( directory );
                }

                if ( file == null )
                {
                    return;
                }

                Factory.Instance.ItemProvider.delete_invalid_file( file );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                Factory.Instance.ItemProvider.delete_file( file );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                Factory.Instance.ItemProvider.delete_folder( folder );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                var treeViewItem = Factory.Instance.Utils.get_visual_parent< TreeViewItem >( button );

                if ( treeViewItem == null )
                {
                    return;
                }

                var fitem = treeViewItem.Header as IFileItem;
                if ( fitem != null )
                {
                    Factory.Instance.ItemProvider.rename_file_item( fitem );
                }
                else
                {
                    var header = treeViewItem.Header as IDirectoryItem;

                    if ( header != null )
                    {
                        Factory.Instance.ItemProvider.rename_directory_items( header );
                    }
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void CheckNamesButton_Click( object sender, RoutedEventArgs e )
        {
            this.check_names();
        }

        private void CheckNamesAllButton_OnClick( object sender, RoutedEventArgs e )
        {
            this.check_names_all();
        }

        private void check_names()
        {
            try
            {
                if ( this.SeriesTreeView.SelectedItem == null )
                {
                    return;
                }

                Factory.Instance.Filebot = new Filebot();

                var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
                var file = this.SeriesTreeView.SelectedItem as IFileItem;

                if ( directory != null )
                {
                    if ( directory.Parent != null )
                    {
                        Factory.Instance.Filebot.check_series( ( IDirectoryItem ) directory.Parent );
                    }
                    else
                    {
                        Factory.Instance.Filebot.check_series( directory );
                    }
                }
                else
                {
                    if ( file?.Parent?.Parent != null )
                    {
                        Factory.Instance.Filebot.check_series( ( IDirectoryItem ) file.Parent.Parent );
                    }
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void check_names_all()
        {
            try
            {
                Factory.Instance.Filebot?.stop_worker();
                Factory.Instance.Filebot = new Filebot();
                Factory.Instance.Filebot.check_series_all();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                Factory.Instance.WindowFileBotPp.set_status_text( "Moving files to correct folders" );
                Factory.Instance.LogLines.Enqueue( "Moving files to correct folders" );

                var directory = this.SeriesTreeView.SelectedItem as IDirectoryItem;
                var file = this.SeriesTreeView.SelectedItem as IFileItem;

                if ( directory != null )
                {
                    Factory.Instance.ItemProvider.move_files_to_valid_folders( directory );
                }

                if ( file == null )
                {
                    return;
                }

                Factory.Instance.ItemProvider.move_file_to_valid_folder( file );

                Factory.Instance.WindowFileBotPp.set_status_text( "Files moved, see log for details" );
                Factory.Instance.LogLines.Enqueue( "Files moved, see log for details" );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.Working.Add( fmi );

                var ffmpeg = new FfmpegCorruptWorker();
                ffmpeg.start_scan();
                Factory.Instance.Working.Add( ffmpeg );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.stop_all_workers();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void CheckConnections()
        {
            try
            {
                this.set_status_text( "Checking for internet access.." );
                var connectionChecker = new BackgroundWorker();
                connectionChecker.DoWork += this.connectionChecker_DoWork;
                connectionChecker.RunWorkerCompleted += this.connectionChecker_RunWorkerCompleted;
                connectionChecker.RunWorkerAsync();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void connectionChecker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            try
            {
                this.SelectFolderButton.IsEnabled = true;

                this.set_status_text( "" );

                if ( Factory.Instance.EztvAvailable )
                {
                    Factory.Instance.fetch_eztv_metadata();
                    Factory.Instance.fetch_kat_metadata();
                    return;
                }

                MessageBox.Show( "Unable to access eztv." + Environment.NewLine + "This functionality will be disabled", "Connectivity Issues" );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void connectionChecker_DoWork( object sender, DoWorkEventArgs e )
        {
            try
            {
                Factory.Instance.EztvAvailable = Factory.Instance.Utils.check_for_eztv_connection();
                Factory.Instance.TvdbAvailable = Factory.Instance.Utils.check_for_tvdb_connection();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                    var fname = Factory.Instance.ItemProvider.get_series_name_from_folder( directory );
                    var series = Factory.Instance.Tvdb?.get_series_by_name( fname );

                    if ( series != null )
                    {
                        this.TvdbTab.Content = new UserControlSeriesViewer( series );
                    }
                    else
                    {
                        this.TvdbTab.Content = new UserControlTvdbMissing();
                    }
                }

                if ( file == null )
                {
                    return;
                }

                var name = Factory.Instance.ItemProvider.get_series_name_from_file( file );

                if ( name != null )
                {
                    var series = Factory.Instance.Tvdb?.get_series_by_name( name );
                    if ( series != null )
                    {
                        this.TvdbTab.Content = new UserControlSeriesViewer( series );
                    }
                    else
                    {
                        this.TvdbTab.Content = new UserControlTvdbMissing();
                    }
                }
                else
                {
                    this.TvdbTab.Content = new UserControlTvdbMissing();
                }

                if ( file.Missing )
                {
                    this.MediaTab.Content = new UserControlTvdbMissing();
                    return;
                }

                var mw = new MediaInfoWorker();
                mw.scan_file_one_time( file );

                if ( file.Mediainfo == null )
                {
                    this.MediaTab.Content = new UserControlTvdbMissing();
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void SelectFolderButton_Click( object sender, RoutedEventArgs e )
        {
            try
            {
                var dialog = new VistaFolderBrowserDialog {Description = @"Please select your Series folder.", UseDescriptionForTitle = true};

                var result = dialog.ShowDialog();

                if ( result != System.Windows.Forms.DialogResult.OK )
                {
                    return;
                }

                this.set_ready( false );

                if ( Factory.Instance.MetaDataReady >= 3 )
                {
                    Factory.Instance.MetaDataReady = 1;
                }

                this.PathLabel.Visibility = Visibility.Visible;

                Factory.Instance.ScanLocation = dialog.SelectedPath;
                this.ScanLocationLabel.Content = dialog.SelectedPath;

                Factory.Instance.ItemProvider = new ItemProvider();
                this.SeriesTreeView.DataContext = Factory.Instance.ItemProvider.Items;
                Factory.Instance.ItemProvider.scan_series_folder();

                if ( Factory.Instance.TvdbAvailable == false )
                {
                    MessageBox.Show( "Unable to access thetvdb." + Environment.NewLine + "This functionality will be disabled", "Connectivity Issues" );
                    return;
                }

                Factory.Instance.fetch_tvdb_metadata();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void fetchMetaDataWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            try
            {
                this.MediaTab.Content = new UserControlMediaInfoViewer( this._item.Mediainfo, this._item.FullName );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                var mw = new MediaInfoWorker();
                mw.scan_file_one_time( this._item );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                    Factory.Instance.Utils.open_file( directory.Path );
                }

                if ( file == null )
                {
                    return;
                }

                Factory.Instance.Utils.open_file( file.Parent.Path );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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

                Factory.Instance.Utils.open_file( file.Path );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void StopThreadsMenuItem_OnClick( object sender, RoutedEventArgs e )
        {
            try
            {
                Factory.Instance.stop_all_workers();
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
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
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void SettingsTitleButton_OnClick( object sender, RoutedEventArgs e )
        {
            this.TabControl.SelectedIndex = 3;
        }

        #endregion
    }
}