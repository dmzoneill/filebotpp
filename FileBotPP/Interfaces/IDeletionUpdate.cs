namespace FileBotPP.Interfaces
{
    public interface IDeletionUpdate
    {
        IDirectoryItem Directory { get; set; }
        IFileItem File { get; set; }
    }
}