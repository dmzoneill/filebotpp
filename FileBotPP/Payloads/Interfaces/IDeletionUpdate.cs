using FileBotPP.Interfaces;
using FileBotPP.Tree.Interfaces;

namespace FileBotPP.Payloads.Interfaces
{
    public interface IDeletionUpdate
    {
        IDirectoryItem Directory { get; set; }
        IFileItem File { get; set; }
    }
}