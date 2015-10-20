using FileBotPP.Interfaces;
using FileBotPP.Payloads.Interfaces;
using FileBotPP.Tree.Interfaces;

namespace FileBotPP.Payloads
{
    public class ExtraFileUpdate : IExtraFileUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
    }
}