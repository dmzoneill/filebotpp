using FileBotPP.Interfaces;
using FileBotPP.Payloads.Interfaces;
using FileBotPP.Tree.Interfaces;

namespace FileBotPP.Payloads
{
    public class FileInsert : IFileInsert
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
        public int EpisodeNum { get; set; }
    }
}