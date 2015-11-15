namespace FileBotPP.Tree
{
    public interface IDuplicateUpdate
    {
        IDirectoryItem Directory { get; set; }
        IFileItem FileA { get; set; }
        IFileItem FileB { get; set; }
    }
}