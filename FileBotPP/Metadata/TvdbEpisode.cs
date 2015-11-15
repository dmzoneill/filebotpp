namespace FileBotPP.Metadata
{
    public class TvdbEpisode : ITvdbEpisode
    {
        private readonly string _name;
        private readonly int _num;

        public TvdbEpisode( int num, string name )
        {
            this._num = num;
            this._name = name;
        }

        public int get_episode_num()
        {
            return this._num;
        }

        public string get_episode_name()
        {
            return this._name;
        }
    }
}