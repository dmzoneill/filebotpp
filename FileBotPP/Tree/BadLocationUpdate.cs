using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public class BadLocationUpdate : IBadLocationUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
        public string NewPath { get; set; }
    }
}