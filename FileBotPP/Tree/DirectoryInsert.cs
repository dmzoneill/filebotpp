using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public struct DirectoryInsert
    {
        public IDirectoryItem Directory { get; set; }
        public IDirectoryItem SubDirectory { get; set; }
        public int Seasonnum { get; set; }
    }
}