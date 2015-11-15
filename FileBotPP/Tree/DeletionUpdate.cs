namespace FileBotPP.Tree
{
    public class DeletionUpdate : IDeletionUpdate
    {
        public IDirectoryItem Directory { get; set; }
        public IFileItem File { get; set; }
    }
}