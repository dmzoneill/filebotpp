namespace FileBotPP.Tree
{
    public interface IFsPoller
    {
        void start_poller();
        void stop_poller();
        void remove_poller();
    }
}