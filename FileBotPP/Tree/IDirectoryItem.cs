namespace FileBotPP.Tree
{
    public interface IDirectoryItem : IItem
    {
        bool Polling { get; set; }
        IFileItem ContainsFile( string name );
        IDirectoryItem ContainsDirectory( string name );
    }
}