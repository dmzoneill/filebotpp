using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace FileBotPP.Tree
{
    public interface IItemProvider
    {
        ObservableCollection< IItem > Items { get; }
        ConcurrentQueue< IDirectoryInsert > NewDirectoryUpdates { get; }
        ConcurrentQueue< IFileInsert > NewFilesUpdates { get; }
        ConcurrentQueue< IDirectoryItem > DetectedDirectories { get; }
        ConcurrentQueue< IFileItem > DetectedFiles { get; }
        ConcurrentQueue< IBadNameUpdate > BadNameFiles { get; }
        ConcurrentQueue< IExtraFileUpdate > ExtraFiles { get; }
        ConcurrentQueue< IDuplicateUpdate > DuplicateFiles { get; }
        ConcurrentQueue< IBadLocationUpdate > BadLocationFiles { get; }
        ConcurrentQueue< IDeletionUpdate > DirectoryDeletions { get; }
        string get_last_scanned_folder();
        void rename_directory_items( IDirectoryItem directory );
        void rename_file_item( IFileItem fileitem );
        void move_item( IFileItem fileitem );
        void move_item( IDirectoryItem diritem );
        void move_item( IFileItem fileitem, IDirectoryItem parent );
        void insert_item_ordered( IItem item );
        void insert_item_ordered_threadsafe( IItem item );
        void insert_item_ordered( IDirectoryItem parent, IDirectoryItem child, int seasonnum );
        void insert_item_ordered( IDirectoryItem parent, IItem child, int episodenum );
        bool contains_child( IDirectoryItem parent, string childname );
        void refresh_tree_directory( IItem parent, string path );
        void update_model();
        void move_files_to_valid_folders( IDirectoryItem directory );
        void move_file_to_valid_folder( IFileItem fitem );
        void delete_invalid_folder( IDirectoryItem directory );
        void delete_invalid_folder_from_tree( IDirectoryItem directory );
        void delete_invalid_folder_from_filesystem( IDirectoryItem directory );
        void delete_invalid_file( IFileItem file );
        void delete_file( IFileItem file );
        void delete_file_in_memory( IFileItem file );
        void delete_folder( IDirectoryItem directory );
        void delete_folder_in_memory( IDirectoryItem directory );
        void delete_invalid_file_from_tree( IFileItem file );
        void delete_invalid_file_from_filesystem( IFileItem file );
        void folder_scan_update();
        void folder_scan_update_threadsafe();
        void scan_series_folder();
        int Count();
        string get_series_name_from_file( IFileItem fitem );
        string get_series_name_from_folder( IDirectoryItem ditem );
        IFileItem ContainsFile( string name );
        IDirectoryItem ContainsDirectory( string name );
    }
}