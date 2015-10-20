using System;
using System.IO;
using System.Linq;
using FileBotPP.Helpers;
using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public class DirectoryItemWatcher
    {
        /*
        private readonly IDirectoryItem _watchedDirectory;

        public DirectoryItemWatcher( IDirectoryItem directoryItem )
        {
            try
            {
                this._watchedDirectory = directoryItem;

                this.Watcher = new FileSystemWatcher
                {
                    Path = this._watchedDirectory.Path,
                    NotifyFilter = NotifyFilters.Attributes |
                                   NotifyFilters.CreationTime |
                                   NotifyFilters.FileName |
                                   NotifyFilters.DirectoryName |
                                   NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.Security |
                                   NotifyFilters.Size
                };
                this.Watcher.Changed += on_item_provider_file_changed;
                this.Watcher.Created += this.on_item_provider_file_created;
                this.Watcher.Deleted += this.on_item_provider_file_deleted;
                this.Watcher.Renamed += on_item_provider_file_renamed;
                this.Watcher.IncludeSubdirectories = false;
                this.Watcher.EnableRaisingEvents = true;
            }
            catch ( Exception )
            {
                //supressed
            }
        }

        public DirectoryItemWatcher( string directory )
        {
            this.Watcher = new FileSystemWatcher
            {
                Path = directory,
                NotifyFilter = NotifyFilters.Attributes |
                               NotifyFilters.CreationTime |
                               NotifyFilters.FileName |
                               NotifyFilters.DirectoryName |
                               NotifyFilters.LastAccess |
                               NotifyFilters.LastWrite |
                               NotifyFilters.Security |
                               NotifyFilters.Size
            };
            this.Watcher.Changed += on_item_provider_file_changed;
            this.Watcher.Created += this.on_item_provider_file_created;
            this.Watcher.Deleted += this.on_item_provider_file_deleted;
            this.Watcher.Renamed += on_item_provider_file_renamed;
            this.Watcher.IncludeSubdirectories = false;
            this.Watcher.EnableRaisingEvents = true;
        }

        public FileSystemWatcher Watcher { get; }

        public void stop_watcher()
        {
            this.Watcher.Dispose();
        }

        private static void on_item_provider_file_changed( object source, FileSystemEventArgs e )
        {
            Utils.LogLines.Enqueue( "File: " + e.FullPath + " " + e.ChangeType );
            Console.WriteLine( "{0} was changed.", e.FullPath );
        }

        private void on_item_provider_file_created( object source, FileSystemEventArgs e )
        {
            if ( !Directory.Exists( e.FullPath ) )
            {
                return;
            }

            var item = new DirectoryItem {FullName = e.Name, Path = e.FullPath, Parent = this._watchedDirectory};
            //item.start_watch_folder();
            ItemProvider.insert_folder_ordered( item );
        }

        private void on_item_provider_file_deleted( object source, FileSystemEventArgs e )
        {
            foreach ( var item1 in this._watchedDirectory.Items.OfType< IDirectoryItem >() )
            {
                if ( String.Compare( item1.Path, e.FullPath, StringComparison.Ordinal ) == 0 )
                {
                    //ItemProvider.delete_folder( item1 );
                    break;
                }
            }

            foreach ( var item1 in this._watchedDirectory.Items.OfType< IFileItem >() )
            {
                if ( String.Compare( item1.Path, e.FullPath, StringComparison.Ordinal ) == 0 )
                {
                    //ItemProvider.delete_invalid_file_from_tree( item1 );
                    break;
                }
            }
        }

        private static void on_item_provider_file_renamed( object source, RenamedEventArgs e )
        {
            Utils.LogLines.Enqueue( String.Format( "File: {0} renamed to {1}", e.OldFullPath, e.FullPath ) );
            Console.WriteLine( "{0} was renamed.", e.FullPath );
        }
        */
    }
}