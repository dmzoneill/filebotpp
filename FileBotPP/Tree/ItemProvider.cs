using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using FileBotPP.Helpers;

namespace FileBotPP.Tree
{
    public static class ItemSorter
    {
        public static IEnumerable< T > order_by_alpha_numeric<T>( this IEnumerable< T > source, Func< T, string > selector )
        {
            var max =
                source.SelectMany(
                    i => Regex.Matches( selector( i ), @"\d+" ).Cast< Match >().Select( m => ( int? ) m.Value.Length ) ).Max() ?? 0;

            return source.OrderBy( i => Regex.Replace( selector( i ), @"\d+", m => m.Value.PadLeft( max, '0' ) ) );
        }
    }

    public class ItemProvider : IItemProvider
    {
        private BackgroundWorker _folderScanner;
        private IFsPoller _fsPoller;
        private string _lastFolderScanned = "";

        public ItemProvider()
        {
            this.NewDirectoryUpdates = new ConcurrentQueue< IDirectoryInsert >();
            this.NewFilesUpdates = new ConcurrentQueue< IFileInsert >();
            this.DetectedDirectories = new ConcurrentQueue< IDirectoryItem >();
            this.DetectedFiles = new ConcurrentQueue< IFileItem >();
            this.BadNameFiles = new ConcurrentQueue< IBadNameUpdate >();
            this.DuplicateFiles = new ConcurrentQueue< IDuplicateUpdate >();
            this.BadLocationFiles = new ConcurrentQueue< IBadLocationUpdate >();
            this.DirectoryDeletions = new ConcurrentQueue< IDeletionUpdate >();
            this.ExtraFiles = new ConcurrentQueue< IExtraFileUpdate >();
            this.Items = new ObservableCollection< IItem >();
        }

        public ObservableCollection< IItem > Items { get; }
        public ConcurrentQueue< IDirectoryInsert > NewDirectoryUpdates { get; }
        public ConcurrentQueue< IFileInsert > NewFilesUpdates { get; }
        public ConcurrentQueue< IDirectoryItem > DetectedDirectories { get; }
        public ConcurrentQueue< IFileItem > DetectedFiles { get; }
        public ConcurrentQueue< IBadNameUpdate > BadNameFiles { get; }
        public ConcurrentQueue< IExtraFileUpdate > ExtraFiles { get; }
        public ConcurrentQueue< IDuplicateUpdate > DuplicateFiles { get; }
        public ConcurrentQueue< IBadLocationUpdate > BadLocationFiles { get; }
        public ConcurrentQueue< IDeletionUpdate > DirectoryDeletions { get; }

        public string get_last_scanned_folder()
        {
            return this._lastFolderScanned;
        }

        public void rename_directory_items( IDirectoryItem directory )
        {
            var renameitems = new List< IFileItem >();
            foreach ( var item in directory.Items.Where( item => item.BadName ) )
            {
                var fitem = item as IFileItem;
                if ( fitem != null )
                {
                    renameitems.Add( fitem );
                }

                var ditem = item as IDirectoryItem;
                if ( ditem != null )
                {
                    this.rename_directory_items( ditem );
                }
            }

            foreach ( var fitem in renameitems )
            {
                this.rename_file_item( fitem );
            }
        }

        public void rename_file_item( IFileItem fileitem )
        {
            if ( File.Exists( fileitem.Parent.Path + "\\" + fileitem.SuggestedName ) )
            {
                fileitem.NewPath = fileitem.Parent.Path + "\\" + fileitem.SuggestedName;
                fileitem.NewPathExists = true;
                return;
            }

            if ( String.Compare( fileitem.SuggestedName, "", StringComparison.Ordinal ) == 0 )
            {
                return;
            }

            var invalid = new string( Path.GetInvalidFileNameChars() ) + new string( Path.GetInvalidPathChars() );

            foreach ( var c in invalid )
            {
                fileitem.SuggestedName = fileitem.SuggestedName.Replace( c.ToString(), "" );
            }

            fileitem.FullName = fileitem.SuggestedName;
            fileitem.Extension = fileitem.SuggestedName.Split( '.' ).Last();
            fileitem.NewPath = fileitem.Parent.Path + "\\" + fileitem.SuggestedName;
            fileitem.ShortName = fileitem.SuggestedName.Substring( 0, fileitem.SuggestedName.LastIndexOf( '.' ) );

            File.Move( fileitem.Path, fileitem.NewPath );

            fileitem.SuggestedName = "";
            fileitem.Path = fileitem.NewPath;
            fileitem.NewPath = "";
            fileitem.BadName = false;

            this.check_is_right_Location( fileitem );

            fileitem.Update();
            this.move_item( fileitem );
        }

        public void move_item( IFileItem fileitem )
        {
            var episodenum = 0;

            try
            {
                var fepnum = Regex.Match( fileitem.FullName, @"(\d+)x(\d+)" );
                if ( fepnum.Success )
                {
                    episodenum = Int32.Parse( fepnum.Groups[ 2 ].Value );
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }

            if ( fileitem.Parent == null )
            {
                return;
            }

            var addpoint = 0;
            foreach ( var t in fileitem.Parent.Items )
            {
                if ( t.FullName.ToLower().Contains( "special" ) )
                {
                    addpoint++;
                    continue;
                }

                var seepnum = Regex.Match( t.FullName, @"(\d+)x(\d+)" );

                if ( seepnum.Success == false )
                {
                    addpoint++;
                    continue;
                }

                var epnum = Int32.Parse( seepnum.Groups[ 2 ].Value );

                if ( episodenum > epnum )
                {
                    addpoint++;
                    continue;
                }

                break;
            }

            fileitem.Parent.Items.Insert( addpoint, fileitem );
            fileitem.Parent.Items.Remove( fileitem );
            fileitem.Update();

            IItem missingitem = null;

            foreach ( var fitem in fileitem.Parent.Items.Where( fitem => fitem.Missing && String.Compare( fitem.FullName, fileitem.ShortName, StringComparison.Ordinal ) == 0 ) )
            {
                missingitem = fitem;
                break;
            }


            if ( missingitem != null )
            {
                fileitem.Parent.Items.Remove( missingitem );
                fileitem.Parent.Update();
            }
        }

        public void move_item( IDirectoryItem diritem )
        {
            var items = diritem.Parent == null ? this.Items : diritem.Parent.Items;
            items.Remove( diritem );

            var addpoint = 0;
            foreach ( var t in items )
            {
                if ( string.Compare( diritem.FullName, t.FullName, StringComparison.Ordinal ) > 0 )
                {
                    addpoint++;
                    continue;
                }
                break;
            }

            if ( addpoint == items.Count )
            {
                items.Add( diritem );
                diritem.Update();
                return;
            }

            items.Insert( addpoint, diritem );
            diritem.Update();
        }

        public void move_item( IFileItem fileitem, IDirectoryItem parent )
        {
            var episodenum = 0;

            try
            {
                var fepnum = Regex.Match( fileitem.FullName, @"(\d+)x(\d+)" );
                if ( fepnum.Success )
                {
                    episodenum = Int32.Parse( fepnum.Groups[ 2 ].Value );
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }

            if ( parent == null )
            {
                return;
            }

            var addpoint = 0;
            foreach ( var t in parent.Items )
            {
                if ( t.FullName.ToLower().Contains( "special" ) )
                {
                    addpoint++;
                    continue;
                }

                var seepnum = Regex.Match( t.FullName, @"(\d+)x(\d+)" );

                if ( seepnum.Success == false )
                {
                    addpoint++;
                    continue;
                }

                var epnum = Int32.Parse( seepnum.Groups[ 2 ].Value );

                if ( episodenum > epnum )
                {
                    addpoint++;
                    continue;
                }

                break;
            }

            IItem missingitem = null;

            foreach ( var fitem in fileitem.Parent.Items.Where( fitem => fitem.Missing && String.Compare( fitem.FullName, fileitem.ShortName, StringComparison.Ordinal ) == 0 ) )
            {
                missingitem = fitem;
                break;
            }


            if ( missingitem != null )
            {
                fileitem.Parent.Items.Remove( missingitem );
            }

            fileitem.Parent.Items.Insert( addpoint, fileitem );
        }

        public void insert_item_ordered( IItem item )
        {
            var whichitems = item.Parent == null ? this.Items : item.Parent.Items;
            var point = whichitems.Select( t => t.FullName ).TakeWhile( entryname => String.Compare( entryname, item.FullName, StringComparison.Ordinal ) <= 0 ).Count();

            if ( point == 0 )
            {
                if ( whichitems.Count > 0 )
                {
                    whichitems.Insert( point, item );
                }
                else
                {
                    whichitems.Add( item );
                }
            }
            else
            {
                if ( point >= whichitems.Count )
                {
                    whichitems.Add( item );
                }
                else
                {
                    whichitems.Insert( point, item );
                }
            }
        }

        public void insert_item_ordered_threadsafe( IItem item )
        {
            Factory.Instance.WindowFileBotPp.Dispatcher.Invoke( ( MethodInvoker ) delegate { insert_item_ordered( item ); } );
        }

        public void insert_item_ordered( IDirectoryItem parent, IDirectoryItem child, int seasonnum )
        {
            var addpoint = 0;
            for ( var x = 0; x < parent.Items.Count; x++ )
            {
                var entryname = parent.Items[ x ].FullName.Split( ' ' );

                if ( entryname.Length < 2 )
                {
                    addpoint++;
                    continue;
                }

                if ( Regex.IsMatch( entryname[ 1 ], @"^\d+$", RegexOptions.IgnoreCase ) == false )
                {
                    addpoint++;
                    continue;
                }

                try
                {
                    var entrynum = Int32.Parse( entryname[ 1 ] );

                    if ( entrynum < seasonnum )
                    {
                        addpoint++;
                        continue;
                    }

                    break;
                }
                catch ( Exception ex )
                {
                    Factory.Instance.LogLines.Enqueue( ex.Message );
                    Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                    parent.Items.Insert( addpoint, child );
                    return;
                }
            }


            if ( parent.Items.Count == addpoint )
            {
                parent.Items.Add( child );
            }
            else
            {
                parent.Items.Insert( addpoint, child );
            }
        }

        public void insert_item_ordered( IDirectoryItem parent, IItem child, int episodenum )
        {
            var addpoint = 0;
            foreach ( var t in parent.Items )
            {
                if ( t.FullName.ToLower().Contains( "special" ) )
                {
                    addpoint++;
                    continue;
                }

                var seepnum = Regex.Match( t.FullName, @"(\d+)x(\d+)" );

                if ( seepnum.Success == false )
                {
                    addpoint++;
                    continue;
                }

                var epnum = Int32.Parse( seepnum.Groups[ 2 ].Value );

                if ( episodenum > epnum )
                {
                    addpoint++;
                    continue;
                }

                break;
            }

            if ( parent.Items.Count == addpoint )
            {
                parent.Items.Add( child );
            }
            else
            {
                parent.Items.Insert( addpoint, child );
            }
        }

        public bool contains_child( IDirectoryItem parent, string childname )
        {
            return parent.Items.Any( child => String.Compare( child.FullName, childname, StringComparison.Ordinal ) == 0 );
        }

        public void refresh_tree_directory( IItem parent, string path )
        {
            try
            {
                var dirInfo = new DirectoryInfo( path );

                var directories = dirInfo.GetDirectories().order_by_alpha_numeric( x => x.Name ).ToArray();

                foreach ( var directory in directories )
                {
                    var item = new DirectoryItem {FullName = directory.Name, Path = directory.FullName, Parent = parent, Polling = true};

                    this.DetectedDirectories.Enqueue( item );
                    this._lastFolderScanned = directory.FullName;

                    Thread.Sleep( 5 );
                    this.refresh_tree_directory( item, item.Path );
                }

                var files = dirInfo.GetFiles().order_by_alpha_numeric( x => x.Name ).ToArray();

                foreach ( var file in files )
                {
                    var shortname = file.Name;
                    var extension = "";

                    if ( file.Name.Contains( "." ) )
                    {
                        shortname = file.Name.Substring( 0, file.Name.LastIndexOf( ".", StringComparison.Ordinal ) );
                        extension = file.Name.Substring( file.Name.LastIndexOf( ".", StringComparison.Ordinal ) + 1 );
                    }

                    var item = new FileItem
                    {
                        FullName = file.Name,
                        ShortName = shortname,
                        Extension = extension,
                        Path = file.FullName,
                        Parent = parent
                    };

                    if ( parent == null )
                    {
                        item.BadLocation = true;
                    }

                    this.DetectedFiles.Enqueue( item );
                }
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void update_model()
        {
            IDirectoryInsert dinsert;
            while ( this.NewDirectoryUpdates.TryDequeue( out dinsert ) )
            {
                var exists = dinsert.Directory.Items.Any( item => String.CompareOrdinal( item.FullName, dinsert.SubDirectory.FullName ) == 0 );

                if ( exists )
                {
                    continue;
                }

                this.insert_item_ordered( dinsert.Directory, dinsert.SubDirectory, dinsert.Seasonnum );
                dinsert.Directory.Update();
                dinsert.Directory.Parent?.Update();
                dinsert.Directory.Parent?.Parent?.Update();
            }

            IFileInsert finsert;
            while ( this.NewFilesUpdates.TryDequeue( out finsert ) )
            {
                var exists = finsert.Directory.Items.OfType< IFileItem >().Any( fitem => String.CompareOrdinal( fitem.FullName, finsert.File.FullName ) == 0 );

                if ( exists )
                {
                    continue;
                }

                this.insert_item_ordered( finsert.Directory, finsert.File, finsert.EpisodeNum );
                finsert.Directory.Update();
                finsert.Directory.Parent?.Update();
                finsert.Directory.Parent?.Parent?.Update();
            }


            IBadNameUpdate bnupdate;

            while ( this.BadNameFiles.TryDequeue( out bnupdate ) )
            {
                if ( bnupdate.File == null )
                {
                    bnupdate.Directory.BadName = true;
                    bnupdate.Directory.SuggestedName = bnupdate.SuggestName;
                    bnupdate.Directory.Update();
                    bnupdate.Directory.Parent?.Update();
                    continue;
                }

                bnupdate.File.BadName = true;
                bnupdate.File.SuggestedName = bnupdate.SuggestName;

                bnupdate.File.Update();
                bnupdate.Directory.Update();
                bnupdate.Directory.Parent?.Update();
            }

            IDuplicateUpdate dfupdate;

            while ( this.DuplicateFiles.TryDequeue( out dfupdate ) )
            {
                dfupdate.FileA.Duplicate = true;
                dfupdate.FileB.Duplicate = true;

                dfupdate.FileA.Update();
                dfupdate.Directory.Update();
                dfupdate.Directory.Parent?.Update();
            }

            IBadLocationUpdate blupdate;

            while ( this.BadLocationFiles.TryDequeue( out blupdate ) )
            {
                if ( blupdate.File == null )
                {
                    blupdate.Directory.BadLocation = true;
                    blupdate.Directory.NewPath = blupdate.NewPath;
                    blupdate.Directory.Update();
                    blupdate.Directory.Parent?.Update();
                    continue;
                }

                blupdate.File.BadLocation = true;
                blupdate.File.NewPath = blupdate.NewPath;
                blupdate.File.Update();
                blupdate.Directory.Update();
                blupdate.Directory.Parent?.Update();
            }

            IDeletionUpdate ddeltion;

            while ( this.DirectoryDeletions.TryDequeue( out ddeltion ) )
            {
                // delete files here
                ddeltion.Directory.Update();
                ddeltion.Directory.Parent?.Update();
            }

            IExtraFileUpdate extrafile;

            while ( this.ExtraFiles.TryDequeue( out extrafile ) )
            {
                // delete files here
                extrafile.File.Extra = true;
                extrafile.Directory.Update();
                extrafile.Directory.Parent?.Update();
            }

            Factory.Instance.MetaDataReady += 1;
        }

        public void move_files_to_valid_folders( IDirectoryItem directory )
        {
            var subdirs = directory.Parent?.Items;
            var moveitems = new List< IFileItem >();

            if ( subdirs == null )
            {
                if ( directory.Items.OfType< IDirectoryItem >().Any() )
                {
                    foreach ( var seasondir in directory.Items.OfType< IDirectoryItem >() )
                    {
                        moveitems.AddRange( seasondir.Items.OfType< IFileItem >().Where( file => file.BadLocation ) );
                    }
                }

                moveitems.AddRange( directory.Items.OfType< IFileItem >().Where( file => file.BadLocation ) );
            }
            else
            {
                moveitems = ( from item in directory.Items.OfType< IFileItem >() where item.BadLocation select item ).ToList();
            }

            foreach ( var item in moveitems )
            {
                this.move_file_to_valid_folder( item );
            }
        }

        public void move_file_to_valid_folder( IFileItem fitem )
        {
            if ( fitem.NewPath == null || String.Compare( fitem.NewPath, "", StringComparison.Ordinal ) == 0 )
            {
                return;
            }

            var targetDirectory = fitem.NewPath.Split( '\\' ).Reverse().ToArray()[ 1 ];

            var pathparts = fitem.NewPath.Split( '\\' );
            pathparts = pathparts.Take( pathparts.Count() - 1 ).ToArray();
            var newdir = String.Join( "\\", pathparts );

            if ( Directory.Exists( newdir ) == false )
            {
                Directory.CreateDirectory( newdir );
                var newdirentry = new DirectoryItem {FullName = targetDirectory, Path = newdir, Parent = fitem.Parent, Polling = true};
                this.insert_item_ordered( newdirentry );
            }

            if ( fitem.Parent?.Parent != null )
            {
                foreach ( var season in fitem.Parent.Parent.Items.OfType< IDirectoryItem >().Where( seasons => String.Compare( seasons.FullName, targetDirectory, StringComparison.Ordinal ) == 0 ) )
                {
                    if ( File.Exists( fitem.NewPath ) )
                    {
                        fitem.NewPathExists = true;
                        return;
                    }

                    File.Move( fitem.Path, fitem.NewPath );

                    fitem.Parent.Items.Remove( fitem );
                    fitem.Parent.Update();
                    fitem.Path = fitem.NewPath;
                    fitem.NewPath = "";
                    fitem.BadLocation = false;
                    fitem.Parent = season;

                    this.move_item( fitem, season );
                    fitem.Update();
                    season.Update();

                    Factory.Instance.WindowFileBotPp.set_status_text( "Moved file: " + fitem.FullName + " to " + fitem.Path );
                    Factory.Instance.LogLines.Enqueue( "Moved file: " + fitem.FullName + " to " + fitem.Path );
                    break;
                }
            }

            if ( fitem.Parent == null )
            {
                return;
            }

            foreach ( var season in fitem.Parent.Items.OfType< IDirectoryItem >().Where( seasons => String.Compare( seasons.FullName, targetDirectory, StringComparison.Ordinal ) == 0 ) )
            {
                if ( File.Exists( fitem.NewPath ) )
                {
                    fitem.NewPathExists = true;
                    return;
                }

                File.Move( fitem.Path, fitem.NewPath );

                fitem.Parent.Items.Remove( fitem );
                fitem.Parent.Update();
                fitem.Path = fitem.NewPath;
                fitem.NewPath = "";
                fitem.BadLocation = false;
                fitem.Parent = season;

                this.move_item( fitem, season );
                fitem.Update();
                season.Update();

                Factory.Instance.WindowFileBotPp.set_status_text( "Moved file: " + fitem.FullName + " to " + fitem.Path );
                Factory.Instance.LogLines.Enqueue( "Moved file: " + fitem.FullName + " to " + fitem.Path );
                break;
            }
        }

        public void delete_invalid_folder( IDirectoryItem directory )
        {
            this.delete_invalid_folder_from_tree( directory );
        }

        public void delete_invalid_folder_from_tree( IDirectoryItem directory )
        {
            if ( directory.Items.Count == 0 )
            {
                if ( directory.Parent == null )
                {
                    this.Items.Remove( directory );
                    this.delete_invalid_folder_from_filesystem( directory );
                    return;
                }
                directory.Parent?.Items.Remove( directory );
                directory.Update();
                directory.Parent?.Update();
                this.delete_invalid_folder_from_filesystem( directory );
                return;
            }

            var directories = directory.Items.OfType< IDirectoryItem >().ToArray();

            foreach ( var subdirectory in directories.Where( subdirectory => subdirectory.AllowedType == false || subdirectory.Corrupt || subdirectory.Empty ).ToArray() )
            {
                this.delete_invalid_folder_from_tree( subdirectory );
            }

            var files = directory.Items.OfType< IFileItem >().ToArray();

            foreach ( var file in files.Where( file => file.AllowedType == false || file.Corrupt ).ToArray() )
            {
                this.delete_invalid_file_from_tree( file );
            }

            if ( directory.Items.Count != 0 )
            {
                return;
            }

            if ( directory.Parent == null )
            {
                this.Items.Remove( directory );
                this.delete_invalid_folder_from_filesystem( directory );
                return;
            }

            directory.Parent?.Items.Remove( directory );
            directory.Parent?.Update();
            this.delete_invalid_folder_from_filesystem( directory );
        }

        public void delete_invalid_folder_from_filesystem( IDirectoryItem directory )
        {
            try
            {
                Factory.Instance.LogLines.Enqueue( "Deleting folder " + directory.Path );

                Directory.Delete( directory.Path, true );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void delete_invalid_file( IFileItem file )
        {
            this.delete_invalid_file_from_tree( file );
        }

        public void delete_file( IFileItem file )
        {
            this.delete_invalid_file_from_tree( file );
        }

        public void delete_file_in_memory( IFileItem file )
        {
            Factory.Instance.WindowFileBotPp.Dispatcher.Invoke( ( MethodInvoker ) delegate
            {
                file.Parent?.Items.Remove( file );
                file.Parent?.Update();
            } );
        }

        public void delete_folder( IDirectoryItem directory )
        {
            Factory.Instance.WindowFileBotPp.Dispatcher.Invoke( ( MethodInvoker ) delegate
            {
                if ( directory.Parent == null )
                {
                    Items.Remove( directory );
                    directory.Update();
                    delete_invalid_folder_from_filesystem( directory );
                    return;
                }
                directory.Parent?.Items.Remove( directory );
                directory.Update();
                directory.Parent?.Update();
                delete_invalid_folder_from_filesystem( directory );
            } );
        }

        public void delete_folder_in_memory( IDirectoryItem directory )
        {
            Factory.Instance.WindowFileBotPp.Dispatcher.Invoke( ( MethodInvoker ) delegate
            {
                if ( directory.Parent == null )
                {
                    Items.Remove( directory );
                    directory.Update();
                    return;
                }
                directory.Parent?.Items.Remove( directory );
                directory.Update();
                directory.Parent?.Update();
            } );
        }

        public void delete_invalid_file_from_tree( IFileItem file )
        {
            var parent = file.Parent;

            if ( parent == null )
            {
                this.Items.Remove( file );
                this.delete_invalid_file_from_filesystem( file );
                return;
            }

            this.delete_invalid_file_from_filesystem( file );
            parent.Items.Remove( file );
            parent.Update();
        }

        public void delete_invalid_file_from_filesystem( IFileItem file )
        {
            try
            {
                Factory.Instance.LogLines.Enqueue( "Deleting file " + file.Path );
                File.Delete( file.Path );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public void folder_scan_update()
        {
            Factory.Instance.WindowFileBotPp.set_status_text( this.get_last_scanned_folder() );

            IDirectoryItem ditem;

            while ( this.DetectedDirectories.TryDequeue( out ditem ) )
            {
                if ( ditem.Parent == null )
                {
                    this.Items.Add( ditem );
                }
                else
                {
                    ditem.Parent.Items.Add( ditem );
                    ditem.Parent.Update();
                }

                ditem.Update();
                ditem.Polling = true;
            }

            IFileItem fitem;

            while ( this.DetectedFiles.TryDequeue( out fitem ) )
            {
                if ( fitem.Parent == null )
                {
                    this.Items.Add( fitem );
                }
                else
                {
                    fitem.Parent.Items.Add( fitem );
                }

                fitem.Update();
                fitem.Parent?.Update();
                fitem.Parent?.Parent?.Update();
            }

            Factory.Instance.WindowFileBotPp.set_series_count( this.Items.OfType< IDirectoryItem >().Count().ToString() );
            Factory.Instance.WindowFileBotPp.set_season_count( this.Items.OfType< IDirectoryItem >().ToList().Sum( series => series.Items.OfType< IDirectoryItem >().Count( item => item.Empty != true ) ).ToString() );
            Factory.Instance.WindowFileBotPp.set_episode_count( this.Items.OfType< IDirectoryItem >().Sum( item => item.Count ).ToString() );
        }

        public void folder_scan_update_threadsafe()
        {
            Factory.Instance.WindowFileBotPp.Dispatcher.Invoke( ( MethodInvoker ) this.folder_scan_update );
        }

        public void scan_series_folder()
        {
            Factory.Instance.WindowFileBotPp.set_status_text( "Scanning..." );
            this._folderScanner = new BackgroundWorker {WorkerReportsProgress = true};
            this._folderScanner.DoWork += this.FolderScanner_DoWork;
            this._folderScanner.RunWorkerCompleted += this.FolderScanner_RunWorkerCompleted;
            this._folderScanner.ProgressChanged += this.FolderScanner_ProgressChanged;
            this._folderScanner.RunWorkerAsync();
        }

        public int Count()
        {
            return this.Items.Sum( item => item.Count );
        }

        public string get_series_name_from_file( IFileItem fitem )
        {
            var subdir = fitem.Parent;
            var parent = subdir?.Parent;
            return parent?.FullName;
        }

        public string get_series_name_from_folder( IDirectoryItem ditem )
        {
            return ditem.Parent == null ? ditem.FullName : ditem.Parent.FullName;
        }

        public IFileItem ContainsFile( string name )
        {
            foreach ( var item in this.Items.OfType< IFileItem >() )
            {
                if ( String.Compare( item.FullName, name, StringComparison.Ordinal ) == 0 )
                {
                    return item;
                }
            }

            return null;
        }

        public IDirectoryItem ContainsDirectory( string name )
        {
            return this.Items.OfType< IDirectoryItem >().FirstOrDefault( item => String.Compare( item.FullName, name, StringComparison.Ordinal ) == 0 );
        }

        private void check_is_right_Location( IItem item )
        {
            var epnums = Regex.Match( item.FullName, @"(\d+)x(\d+)" );
            int epseasonnum;

            if ( epnums.Success )
            {
                epseasonnum = Int32.Parse( epnums.Groups[ 1 ].Value );
            }
            else
            {
                return;
            }

            var snums = Regex.Match( item.Parent.FullName, @"(\d+)" );
            int sseasonnum;

            if ( snums.Success )
            {
                sseasonnum = Int32.Parse( snums.Groups[ 1 ].Value );
            }
            else
            {
                item.BadLocation = true;
                var checkdir = item.Parent.Path + "\\Season " + epseasonnum;
                item.NewPath = checkdir + "\\" + item.FullName;
                return;
            }

            if ( item.Parent.Parent == null && epseasonnum != sseasonnum )
            {
                item.BadLocation = true;
                var checkdir = item.Parent.Path + "\\Season " + epseasonnum;
                item.NewPath = checkdir + "\\" + item.FullName;
            }
            if ( item.Parent.Parent != null && epseasonnum != sseasonnum )
            {
                item.BadLocation = true;
                var checkdir = item.Parent.Parent.Path + "\\Season " + epseasonnum;
                item.NewPath = checkdir + "\\" + item.FullName;
            }
        }

        private void create_collection_tree( IItem parent, string path )
        {
            try
            {
                var dirInfo = new DirectoryInfo( path );

                var directories = dirInfo.GetDirectories().order_by_alpha_numeric( x => x.Name ).ToArray();

                foreach ( var directory in directories )
                {
                    var item = new DirectoryItem {FullName = directory.Name, Path = directory.FullName, Parent = parent, Polling = true};

                    this.DetectedDirectories.Enqueue( item );

                    Thread.Sleep( 5 );
                    this.create_collection_tree( item, item.Path );
                    this._lastFolderScanned = directory.FullName;
                }

                var files = dirInfo.GetFiles().order_by_alpha_numeric( x => x.Name ).ToArray();

                foreach ( var file in files )
                {
                    var shortname = file.Name;
                    var extension = "";

                    if ( file.Name.Contains( "." ) )
                    {
                        shortname = file.Name.Substring( 0, file.Name.LastIndexOf( ".", StringComparison.Ordinal ) );
                        extension = file.Name.Substring( file.Name.LastIndexOf( ".", StringComparison.Ordinal ) + 1 );
                    }

                    var item = new FileItem
                    {
                        FullName = file.Name,
                        ShortName = shortname,
                        Extension = extension,
                        Path = file.FullName,
                        Parent = parent
                    };

                    if ( parent == null )
                    {
                        item.BadLocation = true;
                    }

                    this.DetectedFiles.Enqueue( item );
                }
                this._folderScanner.ReportProgress( 1 );
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private void FolderScanner_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            this.folder_scan_update();
        }

        private void FolderScanner_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            Factory.Instance.WindowFileBotPp.set_status_text( "Series tree populated..." );
            Factory.Instance.MetaDataReady += 1;
            Factory.Instance.WindowFileBotPp.set_ready( true );
        }

        private void FolderScanner_DoWork( object sender, DoWorkEventArgs e )
        {
            if ( this._fsPoller != null )
            {
                FsPoller.stop_all();
                this._fsPoller = null;
            }

            this.create_collection_tree( null, Factory.Instance.ScanLocation );
            this._fsPoller = new FsPoller();
        }
    }
}