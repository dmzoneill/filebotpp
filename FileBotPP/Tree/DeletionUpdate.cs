using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public struct DeletionUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
    }
}