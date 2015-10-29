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
using FileBotPP.Payloads.Interfaces;
using FileBotPP.Tree.Interfaces;

namespace FileBotPP.Tree
{
    public static class ItemProvider
    {
        private static string _lastFolderScanned = "";
        public static ObservableCollection< IItem > Items;
        public static ConcurrentQueue< IDirectoryInsert > NewDirectoryUpdates;
        public static ConcurrentQueue< IFileInsert > NewFilesUpdates;
        public static ConcurrentQueue< IDirectoryItem > DetectedDirectories;
        public static ConcurrentQueue< IFileItem > DetectedFiles;
        public static ConcurrentQueue< IBadNameUpdate > BadNameFiles;
        public static ConcurrentQueue< IExtraFileUpdate > ExtraFiles;
        public static ConcurrentQueue< IDuplicateUpdate > DuplicateFiles;
        public static ConcurrentQueue< IBadLocationUpdate > BadLocationFiles;
        public static ConcurrentQueue< IDeletionUpdate > DirectoryDeletions;
        private static BackgroundWorker _folderScanner;

        static ItemProvider()
        {
            NewDirectoryUpdates = new ConcurrentQueue< IDirectoryInsert >();
            NewFilesUpdates = new ConcurrentQueue< IFileInsert >();
            DetectedDirectories = new ConcurrentQueue< IDirectoryItem >();
            DetectedFiles = new ConcurrentQueue< IFileItem >();
            BadNameFiles = new ConcurrentQueue< IBadNameUpdate >();
            DuplicateFiles = new ConcurrentQueue< IDuplicateUpdate >();
            BadLocationFiles = new ConcurrentQueue< IBadLocationUpdate >();
            DirectoryDeletions = new ConcurrentQueue< IDeletionUpdate >();
            ExtraFiles = new ConcurrentQueue< IExtraFileUpdate >();
            Items = new ObservableCollection< IItem >();
        }

        public static string get_last_scanned_folder()
        {
            return _lastFolderScanned;
        }

        public static void rename_directory_items( IDirectoryItem directory )
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
                    rename_directory_items( ditem );
                }
            }

            foreach ( var fitem in renameitems )
            {
                rename_file_item( fitem );
            }
        }

        public static void rename_file_item( IFileItem fileitem )
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

            check_is_right_Location( fileitem );

            fileitem.Update();
            move_item( fileitem );
        }

        private static void check_is_right_Location( IItem item )
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

        public static void move_item( IFileItem fileitem )
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
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
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

        public static void move_item( IDirectoryItem diritem )
        {
            var items = diritem.Parent == null ? Items : diritem.Parent.Items;
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

        public static void move_item( IFileItem fileitem, IDirectoryItem parent )
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
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
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

        public static void insert_folder_ordered( IDirectoryItem item )
        {
            Common.FileBotPp.Dispatcher.Invoke( ( MethodInvoker ) delegate
            {
                var whichitems = item.Parent == null ? Items : item.Parent.Items;
                var point = 0;
                for ( var x = 0; x < whichitems.Count; x++ )
                {
                    var entryname = whichitems[ x ].FullName;
                    if ( String.Compare( entryname, item.FullName, StringComparison.Ordinal ) > 0 )
                    {
                        point = x;
                        break;
                    }
                }

                if ( point == 0 )
                {
                    whichitems.Add( item );
                }
                else
                {
                    whichitems.Insert( point, item );
                }
            } );
        }

        public static void insert_item_ordered( IDirectoryItem parent, IDirectoryItem child, int seasonnum )
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
                    Utils.LogLines.Enqueue( ex.Message );
                    Utils.LogLines.Enqueue( ex.StackTrace );
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

        public static void insert_item_ordered( IDirectoryItem parent, IItem child, int episodenum )
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

        public static bool contains_child( IDirectoryItem parent, string childname )
        {
            return parent.Items.Any( child => String.Compare( child.FullName, childname, StringComparison.Ordinal ) == 0 );
        }

        private static void create_collection_tree( IItem parent, string path )
        {
            try
            {
                var dirInfo = new DirectoryInfo( path );

                var directories = dirInfo.GetDirectories().order_by_alpha_numeric( x => x.Name ).ToArray();

                foreach ( var directory in directories )
                {
                    var item = new DirectoryItem {FullName = directory.Name, Path = directory.FullName, Parent = parent};

                    DetectedDirectories.Enqueue( item );

                    Thread.Sleep( 5 );
                    create_collection_tree( item, item.Path );
                    _lastFolderScanned = directory.FullName;
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

                    DetectedFiles.Enqueue( item );
                }
                _folderScanner.ReportProgress( 1 );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public static void refresh_tree_directory( IItem parent, string path )
        {
            try
            {
                var dirInfo = new DirectoryInfo( path );

                var directories = dirInfo.GetDirectories().order_by_alpha_numeric( x => x.Name ).ToArray();

                foreach ( var directory in directories )
                {
                    var item = new DirectoryItem {FullName = directory.Name, Path = directory.FullName, Parent = parent};

                    DetectedDirectories.Enqueue( item );
                    _lastFolderScanned = directory.FullName;

                    Thread.Sleep( 5 );
                    refresh_tree_directory( item, item.Path );
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

                    DetectedFiles.Enqueue( item );
                }
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public static void update_model()
        {
            IDirectoryInsert dinsert;
            while ( NewDirectoryUpdates.TryDequeue( out dinsert ) )
            {
                var exists = false;

                foreach ( var item in dinsert.Directory.Items )
                {
                    if ( String.CompareOrdinal( item.FullName, dinsert.SubDirectory.FullName ) == 0 )
                    {
                        exists = true;
                        break;
                    }
                }

                if ( exists )
                {
                    continue;
                }

                insert_item_ordered( dinsert.Directory, dinsert.SubDirectory, dinsert.Seasonnum );
                dinsert.Directory.Update();
                dinsert.Directory.Parent?.Update();
                dinsert.Directory.Parent?.Parent?.Update();
            }

            IFileInsert finsert;
            while ( NewFilesUpdates.TryDequeue( out finsert ) )
            {
                var exists = false;

                foreach ( var fitem in finsert.Directory.Items.OfType< IFileItem >() )
                {
                    if ( String.CompareOrdinal( fitem.FullName, finsert.File.FullName ) == 0 )
                    {
                        exists = true;
                        break;
                    }
                }

                if ( exists )
                {
                    continue;
                }

                insert_item_ordered( finsert.Directory, finsert.File, finsert.EpisodeNum );
                finsert.Directory.Update();
                finsert.Directory.Parent?.Update();
                finsert.Directory.Parent?.Parent?.Update();
            }


            IBadNameUpdate bnupdate;

            while ( BadNameFiles.TryDequeue( out bnupdate ) )
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

            while ( DuplicateFiles.TryDequeue( out dfupdate ) )
            {
                dfupdate.FileA.Duplicate = true;
                dfupdate.FileB.Duplicate = true;

                dfupdate.FileA.Update();
                dfupdate.Directory.Update();
                dfupdate.Directory.Parent?.Update();
            }

            IBadLocationUpdate blupdate;

            while ( BadLocationFiles.TryDequeue( out blupdate ) )
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

            while ( DirectoryDeletions.TryDequeue( out ddeltion ) )
            {
                // delete files here
                ddeltion.Directory.Update();
                ddeltion.Directory.Parent?.Update();
            }

            IExtraFileUpdate extrafile;

            while ( ExtraFiles.TryDequeue( out extrafile ) )
            {
                // delete files here
                extrafile.File.Extra = true;
                extrafile.Directory.Update();
                extrafile.Directory.Parent?.Update();
            }
        }

        public static void move_files_to_valid_folders( IDirectoryItem directory )
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
                move_file_to_valid_folder( item );
            }
        }

        public static void move_file_to_valid_folder( IFileItem fitem )
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
                insert_folder_ordered( new DirectoryItem {FullName = targetDirectory, Path = newdir, Parent = fitem.Parent} );
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

                    move_item( fitem, season );
                    fitem.Update();
                    season.Update();

                    Common.FileBotPp.set_status_text( "Moved file: " + fitem.FullName + " to " + fitem.Path );
                    Utils.LogLines.Enqueue( "Moved file: " + fitem.FullName + " to " + fitem.Path );
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

                move_item( fitem, season );
                fitem.Update();
                season.Update();

                Common.FileBotPp.set_status_text( "Moved file: " + fitem.FullName + " to " + fitem.Path );
                Utils.LogLines.Enqueue( "Moved file: " + fitem.FullName + " to " + fitem.Path );
                break;
            }
        }

        public static void delete_invalid_folder( IDirectoryItem directory )
        {
            delete_invalid_folder_from_tree( directory );
        }

        public static void delete_invalid_folder_from_tree( IDirectoryItem directory )
        {
            if ( directory.Items.Count == 0 )
            {
                if ( directory.Parent == null )
                {
                    Items.Remove( directory );
                    delete_invalid_folder_from_filesystem( directory );
                    return;
                }
                directory.Parent?.Items.Remove( directory );
                directory.Update();
                directory.Parent?.Update();
                delete_invalid_folder_from_filesystem( directory );
                return;
            }

            var directories = directory.Items.OfType< IDirectoryItem >().ToArray();

            foreach ( var subdirectory in directories.Where( subdirectory => subdirectory.AllowedType == false || subdirectory.Corrupt || subdirectory.Empty ).ToArray() )
            {
                delete_invalid_folder_from_tree( subdirectory );
            }

            var files = directory.Items.OfType< IFileItem >().ToArray();

            foreach ( var file in files.Where( file => file.AllowedType == false || file.Corrupt ).ToArray() )
            {
                delete_invalid_file_from_tree( file );
            }

            if ( directory.Items.Count != 0 )
            {
                return;
            }

            if ( directory.Parent == null )
            {
                Items.Remove( directory );
                delete_invalid_folder_from_filesystem( directory );
                return;
            }

            directory.Parent?.Items.Remove( directory );
            directory.Parent?.Update();
            delete_invalid_folder_from_filesystem( directory );
        }

        public static void delete_invalid_folder_from_filesystem( IDirectoryItem directory )
        {
            try
            {
                Utils.LogLines.Enqueue( "Deleting folder " + directory.Path );

                Directory.Delete( directory.Path, true );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        public static void delete_invalid_file( IFileItem file )
        {
            delete_invalid_file_from_tree( file );
        }

        public static void delete_file( IFileItem file )
        {
            delete_invalid_file_from_tree( file );
        }

        public static void delete_folder( IDirectoryItem directory )
        {
            Common.FileBotPp.Dispatcher.Invoke( ( MethodInvoker ) delegate
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

        public static void delete_invalid_file_from_tree( IFileItem file )
        {
            var parent = file.Parent;

            if ( parent == null )
            {
                Items.Remove( file );
                delete_invalid_file_from_filesystem( file );
                return;
            }

            delete_invalid_file_from_filesystem( file );
            parent.Items.Remove( file );
            parent.Update();
        }

        public static void delete_invalid_file_from_filesystem( IFileItem file )
        {
            try
            {
                Utils.LogLines.Enqueue( "Deleting file " + file.Path );
                File.Delete( file.Path );
            }
            catch ( Exception ex )
            {
                Utils.LogLines.Enqueue( ex.Message );
                Utils.LogLines.Enqueue( ex.StackTrace );
            }
        }

        private static void FolderScanner_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            folder_scan_update();
        }

        public static void folder_scan_update()
        {
            Common.FileBotPp.set_status_text( get_last_scanned_folder() );

            IDirectoryItem ditem;

            while ( DetectedDirectories.TryDequeue( out ditem ) )
            {
                if ( ditem.Parent == null )
                {
                    Items.Add( ditem );
                }
                else
                {
                    ditem.Parent.Items.Add( ditem );
                }

                ditem.Update();
            }

            IFileItem fitem;

            while ( DetectedFiles.TryDequeue( out fitem ) )
            {
                if ( fitem.Parent == null )
                {
                    Items.Add( fitem );
                }
                else
                {
                    fitem.Parent.Items.Add( fitem );
                }

                fitem.Update();
                fitem.Parent?.Update();
                fitem.Parent?.Parent?.Update();
            }

            Common.FileBotPp.set_series_count( Items.OfType< IDirectoryItem >().Count().ToString() );
            Common.FileBotPp.set_season_count( Items.OfType< IDirectoryItem >().ToList().Sum( series => series.Items.OfType< IDirectoryItem >().Count( item => item.Empty != true ) ).ToString() );
            Common.FileBotPp.set_episode_count( Items.OfType< IDirectoryItem >().Sum( item => item.Count ).ToString() );
        }

        private static void FolderScanner_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            Common.FileBotPp.set_status_text( "Series tree populated..." );
            Common.MetaDataReady += 1;
            Common.FileBotPp.set_ready( true );
        }

        private static void FolderScanner_DoWork( object sender, DoWorkEventArgs e )
        {
            create_collection_tree( null, Common.ScanLocation );
        }

        public static void scan_series_folder()
        {
            Common.FileBotPp.set_status_text( "Scanning..." );
            _folderScanner = new BackgroundWorker {WorkerReportsProgress = true};
            _folderScanner.DoWork += FolderScanner_DoWork;
            _folderScanner.RunWorkerCompleted += FolderScanner_RunWorkerCompleted;
            _folderScanner.ProgressChanged += FolderScanner_ProgressChanged;
            _folderScanner.RunWorkerAsync();
        }

        public static int Count()
        {
            return Items.Sum( item => item.Count );
        }

        public static string get_series_name_from_file( IFileItem fitem )
        {
            var subdir = fitem.Parent;
            var parent = subdir?.Parent;
            return parent?.FullName;
        }

        public static string get_series_name_from_folder( IDirectoryItem ditem )
        {
            return ditem.Parent == null ? ditem.FullName : ditem.Parent.FullName;
        }
    }
}