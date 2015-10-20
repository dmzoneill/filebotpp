using FileBotPP.Payloads.Interfaces;
using FileBotPP.Tree.Interfaces;

namespace FileBotPP.Payloads
{
    public class DirectoryInsert : IDirectoryInsert
    {
        public IDirectoryItem Directory { get; set; }
        public IDirectoryItem SubDirectory { get; set; }
        public int Seasonnum { get; set; }
    }
}