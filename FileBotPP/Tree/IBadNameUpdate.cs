namespace FileBotPP.Tree
{
    public interface IBadNameUpdate
    {
        IDirectoryItem Directory { get; set; }
        IFileItem File { get; set; }
        string SuggestName { get; set; }
    }
}