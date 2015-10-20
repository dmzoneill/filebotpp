using FileBotPP.Interfaces;
using FileBotPP.Payloads.Interfaces;
using FileBotPP.Tree.Interfaces;

namespace FileBotPP.Payloads
{
    public class BadNameUpdate : IBadNameUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
        public string SuggestName { get; set; }
    }
}