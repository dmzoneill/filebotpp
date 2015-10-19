using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public class FileInsert : IFileInsert
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
        public int EpisodeNum { get; set; }
    }
}