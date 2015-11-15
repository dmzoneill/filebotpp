namespace FileBotPP.Tree
{
    public interface IDeletionUpdate
    {
        IDirectoryItem Directory { get; set; }
        IFileItem File { get; set; }
    }
}