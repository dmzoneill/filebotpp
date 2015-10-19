using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public struct BadLocationUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
        public string NewPath { get; set; }
    }
}