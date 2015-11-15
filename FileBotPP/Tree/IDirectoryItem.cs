namespace FileBotPP.Tree
{
    public interface IDirectoryItem : IItem
    {
        IFileItem ContainsFile( string name );
        IDirectoryItem ContainsDirectory( string name );
        bool Polling { get; set; }
    }
}