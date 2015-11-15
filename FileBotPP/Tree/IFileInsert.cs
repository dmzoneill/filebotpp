namespace FileBotPP.Tree
{
    public interface IFileInsert
    {
        IDirectoryItem Directory { get; set; }
        IFileItem File { get; set; }
        int EpisodeNum { get; set; }
    }
}