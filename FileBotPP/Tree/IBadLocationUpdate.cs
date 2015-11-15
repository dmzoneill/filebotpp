namespace FileBotPP.Tree
{
    public interface IBadLocationUpdate
    {
        IDirectoryItem Directory { get; set; }
        IFileItem File { get; set; }
        string NewPath { get; set; }
    }
}