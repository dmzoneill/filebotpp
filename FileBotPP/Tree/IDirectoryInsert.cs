namespace FileBotPP.Tree
{
    public interface IDirectoryInsert
    {
        IDirectoryItem Directory { get; set; }
        IDirectoryItem SubDirectory { get; set; }
        int Seasonnum { get; set; }
    }
}