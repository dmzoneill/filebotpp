using FileBotPP.Interfaces;
using FileBotPP.Payloads.Interfaces;
using FileBotPP.Tree.Interfaces;

namespace FileBotPP.Payloads
{
    public class DuplicateUpdate : IDuplicateUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem FileA { get; set; }
        public IFileItem FileB { get; set; }
    }
}