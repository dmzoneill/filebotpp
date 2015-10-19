using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public struct DuplicateUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem FileA { get; set; }
        public IFileItem FileB { get; set; }
    }
}