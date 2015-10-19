using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public struct ExtraFileUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
    }
}