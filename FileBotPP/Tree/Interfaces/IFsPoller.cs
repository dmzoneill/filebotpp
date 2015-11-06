namespace FileBotPP.Tree.Interfaces
{
    public interface IFsPoller
    {
        void start_poller();
        void stop_poller();
        void remove_poller();
    }
}