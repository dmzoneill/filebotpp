using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public struct FileInsert
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
        public int EpisodeNum { get; set; }
    }
}