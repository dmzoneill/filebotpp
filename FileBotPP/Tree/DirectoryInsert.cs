using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public class DirectoryInsert : IDirectoryInsert
    {
        public IDirectoryItem Directory { get; set; }
        public IDirectoryItem SubDirectory { get; set; }
        public int Seasonnum { get; set; }
    }
}