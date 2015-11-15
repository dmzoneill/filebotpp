namespace FileBotPP.Tree
{
    public class ExtraFileUpdate : IExtraFileUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
    }
}