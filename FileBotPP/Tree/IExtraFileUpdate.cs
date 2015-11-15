namespace FileBotPP.Tree
{
    public interface IExtraFileUpdate
    {
        IDirectoryItem Directory { get; set; }
        IFileItem File { get; set; }
    }
}