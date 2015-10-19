using FileBotPP.Interfaces;

namespace FileBotPP.Metadata.tvdb
{
    public class Episode : IEpisode
    {
        private readonly string _name;
        private readonly int _num;

        public Episode( int num, string name )
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